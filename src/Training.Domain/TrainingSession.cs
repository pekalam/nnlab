using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.Domain;
using NNLib;
using Prism.Mvvm;

namespace Training.Domain
{
    public class TrainingSession : BindableBase, INotifyDataErrorInfo
    {
        private bool _isValid;
        private string? _error;

        private CancellationTokenSource? _epochCts;
        private Task _sessionTask = Task.CompletedTask;
        private bool _reseted;
        private bool _stopRequested;

        private TimeSpan _elapsed = TimeSpan.Zero;
        private MLPTrainer? _trainer;
        private TrainingSessionReport? _currentReport;
        private bool _started;
        private bool _stopped;
        private bool _paused;
        private DateTime? _startTime;

        private readonly Session _session;

        public event EventHandler<EpochEndArgs>? EpochEnd;

        public TrainingSession(AppState appState)
        {
            _session = appState.ActiveSession!;

            if(appState.ActiveSession!.TrainingData != null && appState.ActiveSession.TrainingParameters != null && appState.ActiveSession.Network != null)
                ConstructTrainer();
            else
                _session.PropertyChanged += SessionOnPropertyChanged;
        }


        /// <summary>
        /// Test method
        /// </summary>
        protected void RaiseEpochEnd(EpochEndArgs args)
        {
            EpochEnd?.Invoke(this,args);
        }

        private void ConstructTrainer()
        {
            _session.TrainingParameters!.PropertyChanged += TrainingParametersOnPropertyChanged;
            Trainer = new MLPTrainer(_session.Network!, _session.TrainingData!.Sets, SelectAlgorithm(), new QuadraticLossFunction());
            IsValid = true;
        }

        private void TrainingParametersOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingParameters.Algorithm))
            {
                Trainer!.Algorithm = SelectAlgorithm();
            }
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
                _session.PropertyChanged += SessionOnTrainingParametersChanged;
            }
        }

        private void SessionOnTrainingParametersChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.TrainingParameters))
            {
                Trainer!.Algorithm = SelectAlgorithm();
            }
        }


        public List<EpochEndArgs> EpochEndEvents { get; } = new List<EpochEndArgs>();
        public MLPNetwork? Network => _session.Network;
        public TrainingParameters? Parameters => _session.TrainingParameters;
        public TrainingData? TrainingData => _session.TrainingData;

        private AlgorithmBase SelectAlgorithm()
        {
            var parameters = _session.TrainingParameters!;
            return parameters.Algorithm switch
            {
                TrainingAlgorithm.GradientDescent => new GradientDescentAlgorithm(parameters.GDParams.Params),
                TrainingAlgorithm.LevenbergMarquardt => new LevenbergMarquardtAlgorithm(parameters.LMParams.Params),
                _ => throw new NotImplementedException()
            };
        }

        
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

        public MLPTrainer? Trainer
        {
            get => _trainer;
            set => SetProperty(ref _trainer, value);
        }

        public TrainingSessionReport? CurrentReport
        {
            get => _currentReport;
            set => SetProperty(ref _currentReport, value);
        }

        public virtual bool Started
        {
            get => _started;
            private set => SetProperty(ref _started, value);
        }

        public virtual bool Stopped
        {
            get => _stopped;
            private set => SetProperty(ref _stopped, value);
        }

        public virtual bool Paused
        {
            get => _paused;
            set => SetProperty(ref _paused, value);
        }

        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

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
            Debug.Assert(Trainer != null);
            
            Paused = false;
            if (!Started)
            {
                CurrentReport = TrainingSessionReport.CreateStoppedSessionReport(Trainer.Epochs, Trainer.Error, StartTime ?? Time.Now, EpochEndEvents);
                Stopped = true;
                return Task.CompletedTask;
            }
            _stopRequested = true;
            _epochCts!.Cancel();
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

        protected virtual async Task<TrainingSessionReport> InternalStart()
        {
            TrainingSessionReport StoppedPausedOrMaxTime()
            {
                if (_stopRequested)
                {
                    Stopped = true;
                    return TrainingSessionReport.CreateStoppedSessionReport(Trainer.Epochs, Trainer.Error, StartTime!.Value, EpochEndEvents);
                }

                if ((Time.Now - StartTime) + _elapsed > Parameters.MaxLearningTime)
                {
                    return TrainingSessionReport.CreateTimeoutSessionReport(Trainer.Epochs, Trainer.Error, StartTime!.Value, EpochEndEvents);
                }

                return TrainingSessionReport.CreatePausedSessionReport(Trainer.Epochs, Trainer.Error, StartTime!.Value, EpochEndEvents);
            }

            CheckCanStart();

            if (double.IsNaN(Trainer!.Error) && !_reseted)
            {
                return TrainingSessionReport.CreateNaNSessionReport(Trainer.Epochs, Trainer.Error, StartTime!.Value, EpochEndEvents);
            }

            if (Parameters!.MaxLearningTime != TimeSpan.MaxValue)
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
                    return TrainingSessionReport.CreateAlgorithmErrorSessionReport(Trainer.Epochs, error, StartTime!.Value, EpochEndEvents);
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
                    return TrainingSessionReport.CreateMaxEpochSessionReport(Trainer.Epochs, error, StartTime!.Value, EpochEndEvents);
                }

                if (double.IsNaN(error))
                {
                    return TrainingSessionReport.CreateNaNSessionReport(Trainer.Epochs, error, StartTime!.Value, EpochEndEvents);
                }

            } while (error > Parameters.TargetError);

            Stopped = true;
            return TrainingSessionReport.CreateTargetReachedSessionReport(Trainer.Epochs, error, StartTime!.Value, EpochEndEvents);
        }

        public Task<TrainingSessionReport> Start()
        {
            if(!IsValid) throw new InvalidOperationException("Session is in invalid state");

            if (CurrentReport != null) CurrentReport.ValidationError = null;
            StartTime = Time.Now;
            var task = InternalStart();
            Started = true;
            Paused = false;
            var sessionTask = task.ContinueWith(t =>
            {
                _elapsed += Time.Now - StartTime.Value;
                Started = _stopRequested = false;
                if (Stopped)
                {
                    Trainer!.ResetEpochs();
                }
                else Paused = true;
                CurrentReport = t.Result;
                _session.TrainingReports.Add(CurrentReport);
                return CurrentReport;
            });
            _sessionTask = sessionTask;
            return sessionTask;
        }

        public Task<double> RunValidation()
        {
            Debug.Assert(Trainer != null);
            
            return Task.Run(() => Trainer.RunValidation()).ContinueWith(t =>
            {
                if (CurrentReport != null) CurrentReport.ValidationError = t.Result;
                return t.Result;
            });
        }

        public Task<double> RunTest()
        {
            Debug.Assert(Trainer != null);

            return Task.Run(() => Trainer.RunTest()).ContinueWith(t =>
            {
                if (CurrentReport != null) CurrentReport.TestError = t.Result;
                return t.Result;
            });
        }

        public void ResetParameters()
        {
            Debug.Assert(Network != null);
            
            Network.RebuildMatrices();
            _reseted = true;
        }






        public IEnumerable GetErrors(string propertyName)
        {
            return new[] {_error};
        }

        public bool HasErrors => _error != null;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void RaiseErrorsChanged([CallerMemberName] string prop = "")
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
        }
    }
}
