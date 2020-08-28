using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Domain;
using NNLib;
using Prism.Mvvm;

namespace Training.Domain
{
    public class Class1
    {
    }

    public class ModuleState : BindableBase
    {
        private readonly AppState _appState;
        private readonly Dictionary<Session, TrainingSession> _sessionToTraining = new Dictionary<Session, TrainingSession>();
        private TrainingSession? _activeSession;

        public event EventHandler<(TrainingSession? prev, TrainingSession next)> ActiveSessionChanged; 

        public ModuleState(AppState appState)
        {
            _appState = appState;

            _appState.PropertyChanged += AppStateOnPropertyChanged;
        }

        private void AppStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.ActiveSession))
            {
                _sessionToTraining.TryGetValue(_appState.ActiveSession!, out var trainingSession);
                if (trainingSession == null)
                {
                    ActiveSession = new TrainingSession(_appState);
                    _sessionToTraining[_appState.ActiveSession!] = _activeSession!;
                }
                else
                {
                    ActiveSession = trainingSession;
                }
            }
        }

        public TrainingSession? ActiveSession
        {
            get => _activeSession;
            set
            {
                if(value == null) throw new NullReferenceException("Null training session");
                var temp = _activeSession;
                SetProperty(ref _activeSession, value);
                ActiveSessionChanged?.Invoke(this, (temp, value));
            }
        }
    }

    public class TrainingSession : BindableBase, INotifyDataErrorInfo
    {
        private bool _isValid;
        private string? _error;

        private CancellationTokenSource _epochCts;
        private Task _sessionTask = Task.CompletedTask;
        private bool _reseted;
        private bool _stopRequested;

        private LevenbergMarquardtAlgorithm? _lmAlgorithm;
        private GradientDescentAlgorithm? _gdAlgorithm;

        private readonly Session _session;
        private AppState _appState;

        public List<EpochEndArgs> EpochEndEvents { get; } = new List<EpochEndArgs>();
        public event EventHandler<EpochEndArgs> EpochEnd;

        public TrainingSession(AppState appState)
        {
            _session = appState.ActiveSession!;
            _appState = appState;

            if(appState.ActiveSession!.TrainingData != null && appState.ActiveSession.TrainingParameters != null && appState.ActiveSession.Network != null)
                ConstructTrainer();
            else
                _session.PropertyChanged += SessionOnPropertyChanged;
        }

        private void ConstructTrainer()
        {
            Trainer = new MLPTrainer(_session.Network!, _session.TrainingData!.Sets, SelectAlgorithm(_session.TrainingParameters!), new QuadraticLossFunction());
            IsValid = true;
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            
            if (_session.Network == null)
            {
                _error = "Empty network";
                IsValid = false;
            }
            else if (_session.TrainingData == null)
            {
                _error = "Empty training data";
                IsValid = false;
            }
            else if (_session.TrainingParameters == null)
            {
                _error = "Empty parameters";
                IsValid = false;
            }
            else
            {
                ConstructTrainer();
                _session.PropertyChanged -= SessionOnPropertyChanged;
            }
        }

        public MLPNetwork? Network => _session.Network;
        public TrainingParameters? Parameters => _session.TrainingParameters;
        public TrainingData TrainingData => _session.TrainingData;

        private AlgorithmBase SelectAlgorithm(TrainingParameters trainingParameters)
        {
            return trainingParameters.Algorithm switch
            {
                TrainingAlgorithm.GradientDescent => (_gdAlgorithm = new GradientDescentAlgorithm(trainingParameters.GDParams)),
                TrainingAlgorithm.LevenbergMarquardt => (_lmAlgorithm = new LevenbergMarquardtAlgorithm(trainingParameters.LMParams))
            };
        }

        private TimeSpan _elapsed = TimeSpan.Zero;
        private MLPTrainer _trainer;
        private TrainingSessionReport? _currentReport;
        private bool _started;
        private bool _stopped;
        private bool _paused;

        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (!value) RaiseErrorsChanged();
                else _error = null;
                SetProperty(ref _isValid, value);
            }
        }

        public MLPTrainer Trainer
        {
            get => _trainer;
            set => SetProperty(ref _trainer, value);
        }

        public TrainingSessionReport? CurrentReport
        {
            get => _currentReport;
            set => SetProperty(ref _currentReport, value);
        }

        public bool Started
        {
            get => _started;
            private set => SetProperty(ref _started, value);
        }

        public bool Stopped
        {
            get => _stopped;
            private set => SetProperty(ref _stopped, value);
        }

        public bool Paused
        {
            get => _paused;
            set => SetProperty(ref _paused, value);
        }

        public DateTime StartTime { get; set; }

        private void CheckCanStart()
        {
            if (Started)
            {
                throw new Exception("Session already started");
            }

            if (Stopped)
            {
                throw new Exception("Session is stopped");
            }
        }

        public Task Stop()
        {
            Paused = false;
            if (!Started)
            {
                CurrentReport = TrainingSessionReport.CreateStoppedSessionReport(Trainer.Epochs, Trainer.Error, StartTime);
                Stopped = true;
                return Task.CompletedTask;
            }
            _stopRequested = true;
            _epochCts.Cancel();
            return _sessionTask;
        }

        public Task Pause()
        {
            if (_epochCts == null)
            {
                throw new Exception("Training session not started");
            }
            _epochCts.Cancel();
            Paused = true;
            return _sessionTask;
        }

        private async Task<TrainingSessionReport> InternalStart()
        {
            TrainingSessionReport StoppedPausedOrMaxTime()
            {
                if (_stopRequested)
                {
                    Stopped = true;
                    return TrainingSessionReport.CreateStoppedSessionReport(Trainer.Epochs, Trainer.Error, StartTime);
                }

                if ((DateTime.Now - StartTime) + _elapsed > Parameters.MaxLearningTime)
                {
                    Stopped = true;
                    return TrainingSessionReport.CreateTimeoutSessionReport(Trainer.Epochs, Trainer.Error, StartTime);
                }

                return TrainingSessionReport.CreatePausedSessionReport(Trainer.Epochs, Trainer.Error, StartTime);
            }

            CheckCanStart();

            if (double.IsNaN(Trainer.Error) && !_reseted)
            {
                return TrainingSessionReport.CreateNaNSessionReport(Trainer.Epochs, Trainer.Error, StartTime);
            }

            if (Parameters.MaxLearningTime != TimeSpan.MaxValue)
            {
                _epochCts = new CancellationTokenSource(Parameters.MaxLearningTime);
            }
            else
            {
                _epochCts = new CancellationTokenSource();
            }

            _reseted = false;
            double error = 0;
            do
            {
                try
                {
                    error = await Trainer.DoEpochAsync(_epochCts.Token);
                }
                catch (OperationCanceledException)
                {
                    return StoppedPausedOrMaxTime();
                }
                catch (TrainingCanceledException)
                {
                    return StoppedPausedOrMaxTime();
                }
                catch (AlgorithmFailed)
                {
                    return TrainingSessionReport.CreateAlgorithmErrorSessionReport(Trainer.Epochs, error, StartTime);
                }

                var arg = new EpochEndArgs()
                {
                    Epoch = Trainer.Epochs,
                    Error = error,
                    Iterations = Trainer.Iterations,
                };
                EpochEnd?.Invoke(this, arg);
                EpochEndEvents.Add(arg);
                if (_epochCts.IsCancellationRequested)
                {
                    return StoppedPausedOrMaxTime();
                }

                if (Trainer.Epochs == Parameters.MaxEpochs)
                {
                    Stopped = true;
                    return TrainingSessionReport.CreateMaxEpochSessionReport(Trainer.Epochs, error, StartTime);
                }

                if (double.IsNaN(error))
                {
                    return TrainingSessionReport.CreateNaNSessionReport(Trainer.Epochs, error, StartTime);
                }

            } while (error > Parameters.TargetError);

            Stopped = true;
            return TrainingSessionReport.CreateTargetReachedSessionReport(Trainer.Epochs, error, StartTime);
        }

        public Task<TrainingSessionReport> Start()
        {
            if(!IsValid) throw new InvalidOperationException("Session is in invalid state");

            if (CurrentReport != null) CurrentReport.ValidationError = null;
            StartTime = DateTime.Now;
            var task = InternalStart();
            Started = true;
            Paused = false;
            var sessionTask = task.ContinueWith(t =>
            {
                _elapsed += DateTime.Now - StartTime;
                Started = _stopRequested = false;
                if (Stopped)
                {
                    Trainer.ResetEpochs();
                }
                CurrentReport = t.Result;
                _session.TrainingReports.Add(CurrentReport);
                return CurrentReport;
            });
            _sessionTask = sessionTask;
            return sessionTask;
        }

        public Task<double> RunValidation()
        {
            return Task.Run(() => Trainer.RunValidation()).ContinueWith(t =>
            {
                if (CurrentReport != null) CurrentReport.ValidationError = t.Result;
                return t.Result;
            });
        }

        public Task<double> RunTest()
        {
            return Task.Run(() => Trainer.RunTest()).ContinueWith(t =>
            {
                if (CurrentReport != null) CurrentReport.TestError = t.Result;
                return t.Result;
            });
        }

        public void ResetParameters()
        {
            Network.RebuildMatrices();
            _reseted = true;
        }






        public IEnumerable GetErrors(string propertyName)
        {
            return new[] {_error};
        }

        public bool HasErrors => _error != null;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        private void RaiseErrorsChanged([CallerMemberName] string prop = "")
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
        }
    }
}
