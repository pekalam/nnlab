using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Training.Application.Plots
{
    internal static class GlobalDistributingDispatcher
    {
        private static readonly ConcurrentDictionary<PlotEpochEndConsumer, ConcurrentQueue<Action>> _queues =
            new ConcurrentDictionary<PlotEpochEndConsumer, ConcurrentQueue<Action>>();

        private static Task? _task;
        private static object _lck = new object();
        private static int _toInvoke;
        private static SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        static GlobalDistributingDispatcher()
        {
        }

        public static void Register(PlotEpochEndConsumer consumer)
        {
            if (!_queues.ContainsKey(consumer))
            {
                _queues[consumer] =  new ConcurrentQueue<Action>();
            }
        }

        public static async ValueTask WaitForQueued()
        {
            if (_toInvoke > 0)
            {
                await _sem.WaitAsync();
                _sem.Release();
            }
        }

        public static void Unregister(PlotEpochEndConsumer consumer)
        {
            if (_queues.Remove(consumer, out var queue))
            {
                Interlocked.Add(ref _toInvoke, -queue.Count);
                if (_toInvoke == 0 && _sem.CurrentCount == 0) _sem.Release();
                queue.Clear();
            }
        }

        private static void TryStartBgTask()
        {
            if (_task == null || _task.Status == TaskStatus.RanToCompletion)
            {
                lock (_lck)
                {
                    //lock optimization
                    if (_task != null && _task?.Status != TaskStatus.RanToCompletion) return;

                    _task = Task.Factory.StartNew(async _ =>
                    {
                        bool stop = false;
                        while (!stop)
                        {
                            stop = true;


                            foreach (var queue in _queues.Values)
                            {
                                if (_toInvoke > 0 && _sem.CurrentCount == 1) _sem.Wait();
                                if (queue.TryDequeue(out var action))
                                {
                                    action?.Invoke();
                                    Interlocked.Decrement(ref _toInvoke);
                                    if (_toInvoke == 0 && _sem.CurrentCount == 0) _sem.Release();
                                    stop = false;
                                }
                            }

                            await Task.Delay(33);
                        }
                    }, CancellationToken.None, TaskCreationOptions.LongRunning);
                }
            }
        }

        public static void Call(Action action, PlotEpochEndConsumer consumer)
        {
            _queues[consumer].Enqueue(() =>
            {
                if (System.Windows.Application.Current == null) return;

                System.Windows.Application.Current.Dispatcher.Invoke(action, DispatcherPriority.Background);
            });
            Interlocked.Increment(ref _toInvoke);
            TryStartBgTask();
        }
    }
}