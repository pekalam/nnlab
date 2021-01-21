using Common.Domain;
using NNLib.Exceptions;
using NNLib.LossFunction;
using NNLib.MLP;
using NNLib.Training;
using NNLib.Training.GradientDescent;
using NNLib.Training.LevenbergMarquardt;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.Extensions.Logging;

namespace Training.Domain
{
    public static class TrainingReportAlgorithmAssembler
    {
        public static TrainingReportAlgorithm FromAlgorithmBase(AlgorithmBase algorithm)
        {
            TrainingReportAlgorithm gdAlgorithm(GradientDescentAlgorithm gd)
            {
                if (gd.Params.BatchSize > 1)
                {
                    if (gd.Params.Momentum > 0) return TrainingReportAlgorithm.MomentumGradientDescentBatch;
                    return TrainingReportAlgorithm.GradientDescentBatch;
                }
                else
                {
                    if (gd.Params.Momentum > 0) return TrainingReportAlgorithm.MomentumGradientDescentOnline;
                    return TrainingReportAlgorithm.GradientDescentOnline;
                }
            }


            return algorithm switch
            {
                GradientDescentAlgorithm gd => gdAlgorithm(gd),
                LevenbergMarquardtAlgorithm _ => TrainingReportAlgorithm.LevenbergMarquardt,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public class TrainingSession : BindableBase
    {
        private readonly Session _session;
        private TrainingSessionReport? _currentReport;

        private TimeSpan _elapsed = TimeSpan.Zero;

        private CancellationTokenSource? _epochCts;
        private bool _isValid;
        private bool _paused;
        private bool _reseted;
        private Task _sessionTask = Task.CompletedTask;
        private bool _started;
        private DateTime? _startTime;
        private bool _stopped;
        private MLPTrainer? _trainer;
        private int _validationThresholdCount;

        private bool _stopRequested;

        public TrainingSession(AppState appState)
        {
            _session = appState.ActiveSession!;

            if (appState.ActiveSession!.TrainingData != null && appState.ActiveSession.TrainingParameters != null &&
                appState.ActiveSession.Network != null)
            {
                ConstructTrainer();
                _session.PropertyChanged += SessionOnTrainingParametersChanged;
            }
            else
                _session.PropertyChanged += SessionOnPropertyChanged;
        }


        public List<EpochEndArgs> EpochEndEvents { get; } = new List<EpochEndArgs>(100_000);
        public MLPNetwork? Network => _session.Network;
        public TrainingParameters? Parameters => _session.TrainingParameters;
        public TrainingData? TrainingData => _session.TrainingData;


        public bool IsValid
        {
            get => _isValid;
            set => SetProperty(ref _isValid, value);
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

        public event EventHandler<EpochEndArgs>? EpochEnd;

        public event Action? TrainerUpdated;
        public event Action? SessionReset;

        private void SessionOnNetworkStructureChanged(MLPNetwork obj)
        {
            if (_trainer!.TrainingSets != _session.TrainingData!.Sets)
            {
                _trainer!.TrainingSets = _session.TrainingData!.Sets;
                TrainerUpdated?.Invoke();
            }
        }

        private void TrainingDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingData.Sets))
            {
                if (_trainer!.Algorithm is GradientDescentAlgorithm gd)
                {
                    if (gd.Params.BatchSize > 1 &&
                        gd.Params.BatchSize != _session.TrainingData!.Sets.TrainingSet.Input.Count)
                    {
                        _session.TrainingParameters!.GDParams.BatchSize =
                            _session.TrainingData.Sets.TrainingSet.Input.Count;
                    }
                }

                // if current validation set is set to null - clear all current epoch end events and reports containing validation error
                if (_trainer.TrainingSets.ValidationSet != null && _session.TrainingData!.Sets.ValidationSet == null)
                {
                    EpochEndEvents.Clear();
                    foreach (var report in _session.TrainingReports.Where(r => r.EpochEndEventArgs[0].ValidationError.HasValue).ToArray())
                    {
                        _session.TrainingReports.Remove(report);
                    }

                    CurrentReport = _session.TrainingReports.Count > 0 ? _session.TrainingReports[^1] : null;
                }


                _trainer!.TrainingSets = _session.TrainingData!.Sets;
                TrainerUpdated?.Invoke();
            }else if (e.PropertyName == nameof(TrainingData.NormalizationMethod))
            {
                _trainer!.TrainingSets = _session.TrainingData!.Sets;
                TrainerUpdated?.Invoke();
            }
        }

        private void ConstructTrainer()
        {
            _session.NetworkStructureChanged += SessionOnNetworkStructureChanged;
            _session.TrainingData!.PropertyChanged -= TrainingDataOnPropertyChanged;
            _session.TrainingData!.PropertyChanged += TrainingDataOnPropertyChanged;
            Trainer = new MLPTrainer(_session.Network!, _session.TrainingData!.Sets, SelectAlgorithm(),
                SelectLossFunction());
            IsValid = true;
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_session.Network == null)
            {
                IsValid = false;
            }
            else if (_session.TrainingData == null)
            {
                IsValid = false;
            }
            else if (_session.TrainingParameters == null)
            {
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
                Trainer!.LossFunction = SelectLossFunction();
                TrainerUpdated?.Invoke();
            }
        }

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

        private ILossFunction SelectLossFunction()
        {
            return _session.TrainingParameters!.LossFunction switch
            {
                LossFunction.MSE => new QuadraticLossFunction(),
                LossFunction.RMSE => new RootMeanSquareLossFunction(),
                LossFunction.MAE => new MaeLossFunction(),
            };
        }

        private void CheckCanStart()
        {
            if (Started) throw new Exception("Session already started");

            if (Stopped) throw new Exception("Session is stopped");
        }

        public async ValueTask ResetSession()
        {
            if (Started) await Stop();

            _session.ResetNetworkToInitialAndClearReports();
            EpochEndEvents.Clear();
            Trainer = new MLPTrainer(_session.Network!, _session.TrainingData!.Sets, SelectAlgorithm(), SelectLossFunction());
            IsValid = true;
            StartTime = null;
            Stopped = Paused = false;
            CurrentReport = null;
            SessionReset?.Invoke();
        }

        private TrainingSessionReport CreateReport(SessionEndType type, DateTime startTime, double? validationError = null)
        {
            return new TrainingSessionReport(type, Trainer!.Epochs, Trainer.Error, startTime, Time.Now - startTime, EpochEndEvents, Network!, TrainingReportAlgorithmAssembler.FromAlgorithmBase(Trainer.Algorithm), validationError);
        }
        
        private TrainingSessionReport CreateReport(SessionEndType type, DateTime startTime, double error, double? validationError = null)
        {
            return new TrainingSessionReport(type, Trainer!.Epochs, error, startTime, Time.Now - startTime, EpochEndEvents, Network!, TrainingReportAlgorithmAssembler.FromAlgorithmBase(Trainer.Algorithm), validationError);
        }

        public Task Stop()
        {
            Debug.Assert(Trainer != null);

            Paused = false;
            if (!Started)
            {
                var report = CreateReport(SessionEndType.Stopped, _session.TrainingReports.Count switch
                {
                    0 => StartTime ?? Time.Now,
                    _ => _session.TrainingReports[^1].EndDate,
                });
                Stopped = true;
                CurrentReport = report;
                lock (_session)
                {
                    _session.TrainingReports.Add(report);
                }
                return Task.CompletedTask;
            }

            _stopRequested = true;
            _epochCts!.Cancel();
            return _sessionTask;
        }

        public Task Pause()
        {
            if (_epochCts == null) throw new Exception("Training session not started");

            _epochCts.Cancel();
            return _sessionTask;
        }

        public Task<TrainingSessionReport> Start()
        {
            if (!IsValid) throw new InvalidOperationException("Session is in invalid state");
            CheckCanStart();

            if (CurrentReport != null) CurrentReport.ValidationError = null;
            StartTime = Time.Now;
            Started = true;
            Paused = false;
            var sessionTask = InternalStart().ContinueWith(t =>
            {
                _elapsed += Time.Now - StartTime.Value;

                if (!t.IsFaulted)
                {
                    CurrentReport = t.Result;
                    if (Parameters!.AddReportOnPause || _stopRequested)
                    {
                        lock (_session)
                        {
                            _session.TrainingReports.Add(CurrentReport);
                        }
                    }
                }
                else
                {
                    Log.Logger.LogError(t.Exception, "Error in training session");
                }

                Started = false;

                if (_stopRequested) Stopped = true;
                else Paused = true;

                _stopRequested = false;
                return CurrentReport;
            });
            _sessionTask = sessionTask;
            return sessionTask;
        }

        protected virtual async Task<TrainingSessionReport> InternalStart()
        {
            TrainingSessionReport StoppedPausedOrMaxTime()
            {
                if (_stopRequested)
                {
                    return CreateReport(SessionEndType.Stopped, StartTime!.Value);
                }
                if (Time.Now - StartTime + _elapsed > Parameters!.MaxLearningTime)
                {
                    return CreateReport(SessionEndType.Timeout, StartTime!.Value);
                }

                return CreateReport(SessionEndType.Paused, StartTime!.Value);
            }

            if (double.IsNaN(Trainer!.Error) && !_reseted)
            {
                return CreateReport(SessionEndType.NaNResult, StartTime!.Value);
            }

            if (Parameters!.MaxLearningTime != TimeSpan.MaxValue)
                _epochCts = new CancellationTokenSource(Parameters.MaxLearningTime);
            else
                _epochCts = new CancellationTokenSource();

            _reseted = false;
            double error = 0;
            double? validationError = null;

            return await Task.Factory.StartNew(() =>
            {
                do
                {
                    try
                    {
                        error = Trainer.DoEpoch(_epochCts.Token);
                        if (Parameters.RunValidation)
                        {
                            _validationThresholdCount++;
                            if (_validationThresholdCount == Parameters.ValidationEpochThreshold)
                            {
                                _validationThresholdCount = 0;
                                validationError = Trainer.RunValidation(_epochCts.Token);
                            }
                        }
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
                        return CreateReport(SessionEndType.AlgorithmError, StartTime!.Value, error);
                    }

                    var arg = new EpochEndArgs
                    {
                        Epoch = Trainer.Epochs,
                        Error = error,
                        Iterations = Trainer.Iterations,
                        ValidationError = validationError,
                    };
                    EpochEndEvents.Add(arg);
                    EpochEnd?.Invoke(this, arg);

                    if (_epochCts.IsCancellationRequested) return StoppedPausedOrMaxTime();

                    if (Trainer.Epochs == Parameters.MaxEpochs)
                    {
                        _stopRequested = true;
                        return CreateReport(SessionEndType.MaxEpoch, StartTime!.Value, error);
                    }

                    if (double.IsNaN(error))
                    {
                        return CreateReport(SessionEndType.NaNResult, StartTime!.Value, error);
                    }

                    if (validationError.HasValue && Parameters.StopWhenValidationErrorReached && validationError.Value <= Parameters.ValidationTargetError)
                    {
                        _stopRequested = true;
                        return CreateReport(SessionEndType.ValidationErrorReached, StartTime!.Value, error, validationError.Value);
                    }
                } while (error > Parameters.TargetError);

                _stopRequested = true;
                return CreateReport(SessionEndType.TargetReached, StartTime!.Value, error);
            }, TaskCreationOptions.LongRunning);
        }

        public Task<double> RunValidation(TrainingSessionReport report)
        {
            Debug.Assert(Trainer != null);

            return Task.Run(() => Trainer.RunValidation(report.Network)).ContinueWith(t =>
            {
                report.ValidationError = t.Result;
                return t.Result;
            });
        }

        public Task<double> RunTest(TrainingSessionReport report)
        {
            Debug.Assert(Trainer != null);

            return Task.Run(() => Trainer.RunTest(report.Network)).ContinueWith(t =>
            {
                report.TestError = t.Result;
                return t.Result;
            });
        }

        public void ResetParameters()
        {
            Debug.Assert(Network != null);

            Network.ResetParameters();
            _reseted = true;
        }
    }
}