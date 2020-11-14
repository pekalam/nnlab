using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static SemaphoreSlim _sem = new SemaphoreSlim(1, 1);

        private static int _toInvoke = 0;
        private static bool _waitingForFinish = false;

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
                _waitingForFinish = true;
                if (await _sem.WaitAsync(TimeSpan.FromSeconds(5)))
                {
                    _sem.Release();
                }
                _waitingForFinish = false;
            }
        }

        public static void Unregister(PlotEpochEndConsumer consumer)
        {
            if(_queues.TryRemove(consumer, out var queue))
            {
                Interlocked.Add(ref _toInvoke, -queue.Count);
                queue.Clear();
            }
        }

        private static void TryStartBgTask()
        {
            if (_task == null)
            {
                lock (_lck)
                {
                    //lock optimization
                    if (_task != null) return;

                    _task = Task.Factory.StartNew(async _ =>
                    {
                        bool stop = false;
                        while (!stop)
                        {
                            stop = true;

                            foreach (var queue in _queues.Values)
                            {
                                stop = false;
                                if (queue.TryDequeue(out var action))
                                {
                                    action?.Invoke();
                                    Interlocked.Decrement(ref _toInvoke);
                                }
                            }

                            if (_waitingForFinish && _toInvoke == 0)
                            {
                                _sem.Release();
                            }

                            await Task.Delay(33);
                        }
                        _task = null;
                    }, CancellationToken.None, TaskCreationOptions.LongRunning);
                }
            }
        }

        public static void Call(Action action, PlotEpochEndConsumer consumer, DispatcherPriority dispatcherPriority = DispatcherPriority.Background)
        {
            if (_queues[consumer].Count < _queues.Count)
            {
                Interlocked.Increment(ref _toInvoke);
                if (_sem.CurrentCount == 1) _sem.Wait();
                _queues[consumer].Enqueue(() =>
                {
                    if (System.Windows.Application.Current == null) return;

                    System.Windows.Application.Current.Dispatcher.Invoke(action, dispatcherPriority);
                });
                TryStartBgTask();
            }
        }


        public static void CallCustom(Action action, PlotEpochEndConsumer consumer)
        {
            if (_queues[consumer].Count < _queues.Count)
            {
                Interlocked.Increment(ref _toInvoke);
                if (_sem.CurrentCount == 1) _sem.Wait();
                _queues[consumer].Enqueue(action);
                TryStartBgTask();
            }
        }
    }
}