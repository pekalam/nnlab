using Common.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Training.Domain;

namespace Training.Application.Plots
{
    internal enum PlotEpochEndConsumerType
    {
        Online,
        Buffering
    }


    internal class PlotEpochEndConsumerOptions
    {
        public const int DefOnlineBufferTimeSpan = 33;
        public const int DefOnlineBufferSize = 300;
        public const int DefBufferingBufferSize = 10;
        public const int DefOnlineSynchTimeSpan = 33;

        public int OnlineBufferTimeSpan { get; set; } = DefOnlineBufferTimeSpan;
        public int OnlineBufferSize { get; set; } = DefOnlineBufferSize;
        public int OnlineSynchTimeSpan { get; set; } = DefOnlineSynchTimeSpan;
        public PlotEpochEndConsumerType DefaultConsumerType { get; set; } = PlotEpochEndConsumerType.Online;
        public int BufferingBufferSize { get; set; } = DefBufferingBufferSize;
        public bool UseOnlineSynch { get; set; }

        public bool FlushBufferOnConsumerTypeChange { get; set; } = true;
    }

    internal class PlotEpochEndConsumer
    {
        private IDisposable? _subscription;
        private IDisposable? _bufSub;
        private IDisposable? _onlineSub;


        private ReplaySubject<EpochEndArgs>? _sub;
        private Subject<EpochEndArgs>? _online;
        private Subject<EpochEndArgs>? _buffering;
        private readonly Action<IList<EpochEndArgs>, TrainingSession> _callback;
        Stopwatch stp = new Stopwatch();
        private TrainingSession? _session;
        private readonly ReplaySubject<int> _bufferSizeSub = new ReplaySubject<int>(1);
        private int _bufferSize;
        private PlotEpochEndConsumerType _consumerType;
        private readonly Action<TrainingSession>? _onTrainingStarting;
        private readonly Action<TrainingSession>? _onTrainingStopped;
        private readonly Action<TrainingSession>? _onTrainingPaused;
        private readonly ModuleState _moduleState;

        private readonly PlotEpochEndConsumerOptions _options;

        private readonly List<EpochEndArgs> _onlineSynchList = new List<EpochEndArgs>(2000);
        private DateTime? _lastOnlineSynch;

        public PlotEpochEndConsumer(ModuleState moduleState, Action<IList<EpochEndArgs>, TrainingSession> callback,
            Action<TrainingSession>? onTrainingStarting = null,
            Action<TrainingSession>? onTrainingStopped = null, Action<TrainingSession>? onTrainingPaused = null,
            PlotEpochEndConsumerOptions? options = null)
        {
            _options = options ?? new PlotEpochEndConsumerOptions();
            _consumerType = _options.DefaultConsumerType;
            _bufferSize = _options.BufferingBufferSize;
            _moduleState = moduleState;
            _onTrainingStarting = onTrainingStarting;
            _onTrainingStopped = onTrainingStopped;
            _onTrainingPaused = onTrainingPaused;
            _callback = callback;
        }

        public bool IsRunning { get; private set; }

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

            session.PropertyChanged -= TrainingSessionOnPropertyChanged;
            session.PropertyChanged += TrainingSessionOnPropertyChanged;
        }

        private void TrainingSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var session = (sender as TrainingSession)!;
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
            IsRunning = false;
        }

        private void SubscribeSession(TrainingSession session)
        {
            GlobalDistributingDispatcher.Register(this);
            Subscribe();
            _onTrainingStarting?.Invoke(session);
            session.EpochEnd += Session_EpochEnd;
        }

        public void ForceStop()
        {
            if (_session != null)
            {
                _session.EpochEnd -= Session_EpochEnd;
                _session.PropertyChanged -= TrainingSessionOnPropertyChanged;
            }
            EndSubscriptions();
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

        private void Session_EpochEnd(object? sender, EpochEndArgs e)
        {
            bool onlineSynch = ConsumerType == PlotEpochEndConsumerType.Online && _options.UseOnlineSynch;
            if (onlineSynch)
            {
                _onlineSynchList.Add(e);
                if (_lastOnlineSynch.HasValue)
                {
                    if ((Time.Now - _lastOnlineSynch.Value).TotalMilliseconds >= _options.OnlineSynchTimeSpan)
                    {
                        Debug.WriteLine((Time.Now - _lastOnlineSynch.Value).TotalMilliseconds);
                        Debug.Assert(_session != null);

                        _callback(_onlineSynchList, _session);
                        _onlineSynchList.Clear();
                        _lastOnlineSynch = Time.Now;
                    }

                }
                else
                {
                    _lastOnlineSynch = Time.Now;
                }
            }
            else
            {
                _sub?.OnNext(e);
            }
        }

        private void InitSubscription()
        {
            Debug.Assert(_session != null);
            if (ConsumerType == PlotEpochEndConsumerType.Online)
            {
                if (_bufSub != null)
                {
                    if (_options.FlushBufferOnConsumerTypeChange)
                    {
                        _buffering?.OnCompleted();
                    }
                    _bufSub?.Dispose();
                    _bufSub = null;
                    _buffering = new Subject<EpochEndArgs>();
                }

                if (!_options.UseOnlineSynch)
                {
                    _onlineSub = _online
                        .Buffer(timeSpan: TimeSpan.FromMilliseconds(_options.OnlineBufferTimeSpan), count: _options.OnlineBufferSize)
                        .DelaySubscription(TimeSpan.FromMilliseconds(_options.OnlineBufferTimeSpan))
                        .SubscribeOn(Scheduler.Default)
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

                _bufSub = _buffering.DynamicBuffer(_bufferSizeSub).Subscribe(args =>
                {
                    if (args.Length > 0) _callback(args, _session);
                });
                _bufferSizeSub.OnNext(BufferSize);
            }
        }

        private void Subscribe()
        {
            _sub = new ReplaySubject<EpochEndArgs>(_options.OnlineBufferSize);
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
            IsRunning = true;
        }
    }
}