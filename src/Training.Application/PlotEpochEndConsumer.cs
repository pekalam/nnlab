using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Common.Domain;
using Prism.Commands;
using Prism.Events;
using Training.Domain;
using Training.Interface;

namespace Training.Application
{
    internal static class DynamicBufferExtension
    {
        public static IObservable<T[]> DynamicBuffer<T>(this IObservable<T> source, IObservable<int> bufferSize)
        {
            return Observable.Create<T[]>(observer =>
            {
                object _bufLck = new object();
                T[] buffer = new T[0];
                int currentBufferSize = 0;
                int count = 0;

                bufferSize.Subscribe(newSz =>
                {
                    lock (_bufLck)
                    {
                        if (newSz > currentBufferSize)
                        {
                            var newBuffer = new T[newSz];
                            buffer.CopyTo(newBuffer, 0);
                            buffer = newBuffer;
                            currentBufferSize = newSz;
                        }
                        else if (newSz < currentBufferSize)
                        {
                            currentBufferSize = newSz;

                            if (count >= newSz)
                            {
                                var toSkip = newSz == 0 ? 0 : newSz - 1;

                                var remainder = buffer.Skip(toSkip).Take(count - toSkip).ToArray();
                                if (remainder.Length > 0) observer.OnNext(remainder);

                                var newBuffer = new T[newSz];
                                buffer.Take(toSkip).ToArray().CopyTo(newBuffer, 0);
                                buffer = newBuffer;
                                count = toSkip;
                            }
                            else
                            {
                                var newBuffer = new T[newSz];
                                buffer.Take(count).ToArray().CopyTo(newBuffer, 0);
                                buffer = newBuffer;
                            }
                        }
                    }
                });

                return source.Subscribe(value =>
                {
                    lock (_bufLck)
                    {
                        if (buffer.Length == 0)
                        {
                            observer.OnNext(new[] {value});
                            return;
                        }

                        if (count == currentBufferSize)
                        {
                            observer.OnNext(buffer);
                            count = 0;
                        }

                        buffer[count] = value;
                        count++;
                    }
                }, observer.OnError, () =>
                {
                    if (count > 0)
                    {
                        observer.OnNext(buffer.Take(count).ToArray());
                        count = 0;
                    }

                    observer.OnCompleted();
                });
            });
        }
    }

    internal enum PlotEpochEndConsumerType
    {
        Online,
        Buffering
    }


    internal static class GlobalDistributingDispatcher
    {
        private static readonly Dictionary<PlotEpochEndConsumer, ConcurrentQueue<Action>> _queues =
            new Dictionary<PlotEpochEndConsumer, ConcurrentQueue<Action>>();

        private static Task? _task;
        private static object _lck = new object();

        static GlobalDistributingDispatcher()
        {
        }

        public static void Register(PlotEpochEndConsumer consumer)
        {
            if (!_queues.ContainsKey(consumer))
            {
                _queues.Add(consumer, new ConcurrentQueue<Action>());
            }

        }

        public static void Unregister(PlotEpochEndConsumer consumer)
        {
            _queues.Remove(consumer);
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
                                if (queue.TryDequeue(out var action))
                                {
                                    action?.Invoke();
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
            _queues[consumer].Enqueue(action);
            TryStartBgTask();
        }
    }

    internal class PlotEpochEndConsumer
    {
        private const int LOCATION_CHANGED_DELAY = 150;
        private const int ONLINE_BUFFER_TIME_SPAN = 33;
        private const int ONLINE_BUFFER_SIZE = 120;
        public const int BUFFERING_BUFFER_SIZE = 10;

        private IDisposable? _subscription;
        private IDisposable? _bufSub;
        private IDisposable? _onlineSub;


        private ReplaySubject<EpochEndArgs>? _sub;
        private Subject<EpochEndArgs>? _online;
        private Subject<EpochEndArgs>? _buffering;
        private readonly Queue<EpochEndArgs> _epochEndWaitingQueue = new Queue<EpochEndArgs>();
        private bool _trainingStarted;
        private bool _handleEpochs = true;
        private CancellationTokenSource? _cts;
        private readonly Action<IList<EpochEndArgs>, TrainingSession> _callback;
        Stopwatch stp = new Stopwatch();
        private readonly DelegateCommand _locationChangedCmd;
        private TrainingSession? _session;
        private readonly ReplaySubject<int> _bufferSizeSub = new ReplaySubject<int>(1);
        private int _bufferSize = BUFFERING_BUFFER_SIZE;
        private PlotEpochEndConsumerType _consumerType;
        private readonly Action<TrainingSession>? _onTrainingStarting;
        private readonly Action<TrainingSession>? _onTrainingStopped;
        private readonly Action<TrainingSession>? _onTrainingPaused;
        private readonly ModuleState _moduleState;

        public PlotEpochEndConsumer(ModuleState moduleState, Action<IList<EpochEndArgs>, TrainingSession> callback, Action<TrainingSession>? onTrainingStarting = null,
            Action<TrainingSession>? onTrainingStopped = null, Action<TrainingSession>? onTrainingPaused = null)
        {
            _moduleState = moduleState;
            _onTrainingStarting = onTrainingStarting;
            _onTrainingStopped = onTrainingStopped;
            _onTrainingPaused = onTrainingPaused;
            _locationChangedCmd = new DelegateCommand(OnLocationChanged);
            _callback = callback;
        }

        public void Initialize()
        {
            if (_moduleState.ActiveSession != null) SetTrainingSessionHandlers(_moduleState.ActiveSession);

            _moduleState.ActiveSessionChanged += ModuleStateOnActiveSessionChanged;
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            if (e.prev != null)
            {
                ForceStop();
            }
            SetTrainingSessionHandlers(e.next);
        }

        private void SetTrainingSessionHandlers(TrainingSession session)
        {
            _session = session;
            if (session.Started)
            {
                SubscribeSession(session);
            }
            session.PropertyChanged += TrainingSessionOnPropertyChanged;
        }

        private void TrainingSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var session = sender as TrainingSession;
            switch (e.PropertyName)
            {
                case nameof(TrainingSession.Started):
                    if (session.Started)
                    {
                        SubscribeSession(session);
                    }
                    break;
                case nameof(TrainingSession.Stopped):
                    if (session.Stopped)
                    {
                        _trainingStarted = false;
                        session.EpochEnd -= Session_EpochEnd;
                        _sub?.OnCompleted();
                        EndSubscriptions();
                        _onTrainingStopped?.Invoke(session);
                        GlobalDistributingDispatcher.Unregister(this);
                    }

                    break;
                case nameof(TrainingSession.Paused):
                    if (session.Paused)
                    {
                        _trainingStarted = false;
                        session.EpochEnd -= Session_EpochEnd;
                        _sub?.OnCompleted();
                        EndSubscriptions();
                        _onTrainingPaused?.Invoke(session);
                        GlobalDistributingDispatcher.Unregister(this);
                    }
                    break;
            }
        }

        private void EndSubscriptions()
        {
            _subscription?.Dispose();
            _bufSub?.Dispose();
            _onlineSub?.Dispose();
            _bufSub = _onlineSub = _subscription = null;
        }

        private void SubscribeSession(TrainingSession session)
        {
            GlobalDistributingDispatcher.Register(this);
            Subscribe();
            _onTrainingStarting?.Invoke(session);
            _trainingStarted = true;
            session.EpochEnd += Session_EpochEnd;
        }

        public void SubscribeStartedSession(TrainingSession session)
        {
            if (!session.Started) throw new ArgumentException("Session not started");
            _session = session;
            SubscribeSession(session);
        }

        public void ForceStop()
        {
            if (_session != null) _session.EpochEnd -= Session_EpochEnd;
            EndSubscriptions();

            //TODO
            //_appCommands.LocationChanged.UnregisterCommand(_locationChangedCmd);
            GlobalDistributingDispatcher.Unregister(this);
        }

        public PlotEpochEndConsumerType ConsumerType
        {
            get => _consumerType;
            set
            {
                _consumerType = value;
                //If started
                if (_buffering != null && _online != null)
                {
                    InitSubscription();
                }
            }
        }

        public int BufferSize
        {
            get => _bufferSize;
            set
            {
                _bufferSize = value;
                _bufferSizeSub.OnNext(value);
            }
        }

        private void OnLocationChanged()
        {
            if (!_trainingStarted)
            {
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            _handleEpochs = false;
            Task.Delay(TimeSpan.FromMilliseconds(LOCATION_CHANGED_DELAY), _cts.Token).ContinueWith(task =>
            {
                if (!task.IsCompletedSuccessfully)
                {
                    return;
                }

                while (_epochEndWaitingQueue.Count > 0)
                {
                    var arg = _epochEndWaitingQueue.Dequeue();
                    _sub?.OnNext(arg);
                }

                _handleEpochs = true;
            });
        }

        private void Session_EpochEnd(object? sender, EpochEndArgs e)
        {
            if (_handleEpochs)
            {
                _sub?.OnNext(e);
            }
            else
            {
                _epochEndWaitingQueue.Enqueue(e);
            }
        }

        private void InitSubscription()
        {
            if (ConsumerType == PlotEpochEndConsumerType.Online)
            {
                if (_bufSub != null)
                {
                    _buffering?.OnCompleted();
                    _bufSub?.Dispose();
                    _bufSub = null;
                    _buffering = new Subject<EpochEndArgs>();
                }

                _onlineSub = _online
                    .Buffer(timeSpan: TimeSpan.FromMilliseconds(ONLINE_BUFFER_TIME_SPAN), count: ONLINE_BUFFER_SIZE)
                    .DelaySubscription(TimeSpan.FromMilliseconds(ONLINE_BUFFER_TIME_SPAN)).SubscribeOn(Scheduler.Default)
                    .Subscribe(list =>
                    {
                        //test
                        //stp.Restart();
                        if (list.Count > 0) _callback(list, _session);
                        //stp.Stop();
                        //if(stp.ElapsedMilliseconds > 0)
                        //Debug.WriteLine($"Epoch end consumer time: " + stp.ElapsedMilliseconds);
                    });
            }
            else
            {
                if (_onlineSub != null)
                {
                    _online?.OnCompleted();
                    _onlineSub?.Dispose();
                    _onlineSub = null;
                    _online = new Subject<EpochEndArgs>();
                }

                _bufSub = _buffering.SubscribeOn(Scheduler.Default).DynamicBuffer(_bufferSizeSub).Subscribe(args =>
                {
                    if (args.Length > 0) _callback(args, _session);
                });
            }
        }

        private void Subscribe()
        {
            _sub = new ReplaySubject<EpochEndArgs>(ONLINE_BUFFER_SIZE);
            _online = new Subject<EpochEndArgs>();
            _buffering = new Subject<EpochEndArgs>();

            //TODO switch plot error
            _subscription = _sub.Subscribe(args =>
            {
                if (ConsumerType == PlotEpochEndConsumerType.Online) _online.OnNext(args);
                else if (ConsumerType == PlotEpochEndConsumerType.Buffering) _buffering.OnNext(args);
            }, () =>
            {
                if (ConsumerType == PlotEpochEndConsumerType.Online) _online.OnCompleted();
                else if (ConsumerType == PlotEpochEndConsumerType.Buffering) _buffering.OnCompleted();
            });

            InitSubscription();
        }
    }
}