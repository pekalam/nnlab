using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Common.Domain;
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
        public const int DefOnlineBufferSize = 120;
        public const int DefBufferingBufferSize = 10;

        public int OnlineBufferTimeSpan { get; set; } = DefOnlineBufferTimeSpan;
        public int OnlineBufferSize { get; set; } = DefOnlineBufferSize;
        public PlotEpochEndConsumerType DefaultConsumerType { get; set; } = PlotEpochEndConsumerType.Online;
        public int BufferingBufferSize { get; set; } = DefBufferingBufferSize;
    }

    internal class PlotEpochEndConsumer
    {
        private IDisposable? _subscription;
        private IDisposable? _bufSub;
        private IDisposable? _onlineSub;


        private ReplaySubject<EpochEndArgs>? _sub;
        private Subject<EpochEndArgs>? _online;
        private Subject<EpochEndArgs>? _buffering;
        private readonly Queue<EpochEndArgs> _epochEndWaitingQueue = new Queue<EpochEndArgs>();
        private bool _handleEpochs = true;
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

        public PlotEpochEndConsumer(ModuleState moduleState, Action<IList<EpochEndArgs>, TrainingSession> callback,
            Action<TrainingSession>? onTrainingStarting = null,
            Action<TrainingSession>? onTrainingStopped = null, Action<TrainingSession>? onTrainingPaused = null,
            PlotEpochEndConsumerOptions options = null)
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
        }

        private void SubscribeSession(TrainingSession session)
        {
            GlobalDistributingDispatcher.Register(this);
            Subscribe();
            _onTrainingStarting?.Invoke(session);
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
            ;
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
        }
    }
}