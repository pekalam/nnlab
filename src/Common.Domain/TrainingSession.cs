using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NNLib;

namespace Common.Domain
{
    public class EpochEndArgs
    {
        public int Epoch { get; set; }
        public double Error { get; set; }
        public int Iterations { get; set; }
    }

    public enum SessionEndType
    {
        TargetReached,
        Stopped,
        Timeout,
        MaxEpoch,
        NaNResult,
        AlgorithmError,
        Paused,
    }

    public class TrainingReportsCollection : ObservableCollection<TrainingSessionReport>
    {
        protected override void ClearItems()
        {
            if (Count > 0 && TrainingSessionReport.IsTerminatingSessionType(Items[^1].SessionEndType))
            {
                throw new InvalidOperationException("Cannot clear session reports because last session is of type " + Items[^1].SessionEndType);
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, TrainingSessionReport item)
        {
            if (index != Count) throw new InvalidOperationException("Cannot insert item with index " + index);
            if(Items.Count > 0 && Items[^1].StartDate + Items[^1].Duration > item.StartDate) throw new ArgumentException("Training session is overlapping previous");
            
            base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            throw new InvalidOperationException("Cannot move item in session reports collection");
        }

        protected override void RemoveItem(int index)
        {
            if (index == Count - 1 && TrainingSessionReport.IsTerminatingSessionType(Items[index].SessionEndType))
            {
                throw new InvalidOperationException("Cannot remove report of type " + Items[index].SessionEndType);
            }
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TrainingSessionReport item)
        {
            throw new InvalidOperationException("Cannot replace item in session reports collection");
        }
    }

    public static class Time
    {
        internal static Func<DateTime> TimeProvider = () => DateTime.Now;
        public static DateTime Now => TimeProvider();
    }
    
    public class TrainingSessionReport
    {
        public static bool IsTerminatingSessionType(SessionEndType type) =>
            type == SessionEndType.Stopped || type == SessionEndType.TargetReached;
        
        public SessionEndType SessionEndType { get; }
        public int TotalEpochs { get; }
        public double Error { get; }
        public DateTime StartDate { get; }
        public TimeSpan Duration { get; }
        
        public DateTime EndDate { get; }
        public double? ValidationError { get; set; }
        public double? TestError { get; set; }

        public TrainingSessionReport(SessionEndType sessionEndType, int totalEpochs, double error, DateTime startDate, TimeSpan duration)
        {
            if(totalEpochs < 0) throw new ArgumentException("totalEpochs < 0");
            if(error < 0d) throw new ArgumentException("error < 0");
            SessionEndType = sessionEndType;
            TotalEpochs = totalEpochs;
            Error = error;
            StartDate = startDate;
            Duration = duration;
            EndDate = startDate + duration;
        }

        public static TrainingSessionReport CreateStoppedSessionReport(int totalEpochs, double error, DateTime startTime) =>
            new TrainingSessionReport(SessionEndType.Stopped, totalEpochs, error, startTime, Time.Now - startTime);

        public static TrainingSessionReport CreatePausedSessionReport(int totalEpochs, double error, DateTime startTime) =>
            new TrainingSessionReport(SessionEndType.Paused, totalEpochs, error, startTime, Time.Now - startTime);

        public static TrainingSessionReport CreateTargetReachedSessionReport(int totalEpochs, double error, DateTime startTime) =>
            new TrainingSessionReport(SessionEndType.TargetReached, totalEpochs, error, startTime, Time.Now - startTime);

        public static TrainingSessionReport CreateMaxEpochSessionReport(int totalEpochs, double error, DateTime startTime) =>
            new TrainingSessionReport(SessionEndType.MaxEpoch, totalEpochs, error, startTime, Time.Now - startTime);

        public static TrainingSessionReport CreateTimeoutSessionReport(int totalEpochs, double error, DateTime startTime) =>
            new TrainingSessionReport(SessionEndType.Timeout, totalEpochs, error, startTime, Time.Now - startTime);

        public static TrainingSessionReport CreateNaNSessionReport(int totalEpochs, double error, DateTime startTime) =>
            new TrainingSessionReport(SessionEndType.NaNResult, totalEpochs, error, startTime, Time.Now - startTime);

        public static TrainingSessionReport CreateAlgorithmErrorSessionReport(int totalEpochs, double error, DateTime startTime) =>
            new TrainingSessionReport(SessionEndType.AlgorithmError, totalEpochs, error, startTime, Time.Now - startTime);
    }

    public class TrainingSession
    {
        private CancellationTokenSource _epochCts;
        private Task _sessionTask = Task.CompletedTask;
        private TrainingParameters _trainingParameters;
        private bool _reseted;
        private bool _stopRequested;

        private LevenbergMarquardtAlgorithm? _lmAlgorithm;
        private GradientDescentAlgorithm? _gdAlgorithm;

        public List<EpochEndArgs> EpochEndEvents { get; } = new List<EpochEndArgs>();
        public event EventHandler<EpochEndArgs> EpochEnd;


        public TrainingSession(TrainingData trainingData, TrainingParameters trainingParameters, MLPNetwork network, string sessionName)
        {
            TrainingData = trainingData;
            _trainingParameters = trainingParameters;
            Network = network;
            SessionName = sessionName;
            //TODO cost func opt
            Trainer = new MLPTrainer(network, trainingData.Sets, SelectAlgorithm(trainingParameters), new QuadraticLossFunction());
        }

        private AlgorithmBase SelectAlgorithm(TrainingParameters trainingParameters)
        {
            return trainingParameters.Algorithm switch
            {
                TrainingAlgorithm.GradientDescent => (_gdAlgorithm = new GradientDescentAlgorithm(trainingParameters.GDParams)),
                TrainingAlgorithm.LevenbergMarquardt => (_lmAlgorithm = new LevenbergMarquardtAlgorithm(trainingParameters.LMParams))
            };
        }

        public string SessionName { get; }
        public TrainingData TrainingData { get; }

        public TrainingParameters TrainingParameters
        {
            get => _trainingParameters;
            set
            {
                if (value.Algorithm != _trainingParameters.Algorithm)
                {
                    Trainer.Algorithm = SelectAlgorithm(value);
                }
                else
                {
                    if (_lmAlgorithm != null) _lmAlgorithm.Params = value.LMParams;
                    if (_gdAlgorithm != null)
                        _gdAlgorithm.Params = value.GDParams;
                }
                _trainingParameters = value;
            }
        }

        private TimeSpan _elapsed = TimeSpan.Zero;

        public MLPNetwork Network { get; }
        public MLPTrainer Trainer { get; }
        public bool Started { get; private set; }

        public bool Stopped { get; private set; }

        public DateTime StartTime { get; set; }
        public TrainingSessionReport Report { get; set; }
        

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
            if (!Started)
            {
                Report = TrainingSessionReport.CreateStoppedSessionReport(Trainer.Epochs, Trainer.Error, StartTime);
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

                if ((DateTime.Now - StartTime) + _elapsed > TrainingParameters.MaxLearningTime)
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

            if (TrainingParameters.MaxLearningTime != TimeSpan.MaxValue)
            {
                _epochCts = new CancellationTokenSource(TrainingParameters.MaxLearningTime);
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
                    return TrainingSessionReport.CreateAlgorithmErrorSessionReport(Trainer.Epochs,error, StartTime);
                }

                var arg = new EpochEndArgs()
                {
                    Epoch = Trainer.Epochs,
                    Error = error,
                    Iterations = Trainer.Iterations,
                };
                EpochEnd?.Invoke(this,arg);
                EpochEndEvents.Add(arg);
                if (_epochCts.IsCancellationRequested)
                {
                    return StoppedPausedOrMaxTime();
                }

                if (Trainer.Epochs == TrainingParameters.MaxEpochs)
                {
                    Stopped = true;
                    return TrainingSessionReport.CreateMaxEpochSessionReport(Trainer.Epochs, error, StartTime);
                }

                if (double.IsNaN(error))
                {
                    return TrainingSessionReport.CreateNaNSessionReport(Trainer.Epochs, error, StartTime);
                }

            } while (error > TrainingParameters.TargetError);

            Stopped = true;
            return TrainingSessionReport.CreateTargetReachedSessionReport(Trainer.Epochs, error, StartTime);
        }

        public Task<TrainingSessionReport> Start()
        {
            if(Report != null) Report.ValidationError = null;
            StartTime = DateTime.Now;
            var task = InternalStart();
            Started = true;
            var sessionTask = task.ContinueWith(t =>
            {
                _elapsed += DateTime.Now - StartTime;
                Started = _stopRequested = false;
                if (Stopped)
                {
                    Trainer.ResetEpochs();
                }
                return Report = t.Result;
            });
            _sessionTask = sessionTask;
            return sessionTask;
        }

        public Task<double> RunValidation()
        {
            return Task.Run(() => Trainer.RunValidation()).ContinueWith(t =>
            {
                if (Report != null) Report.ValidationError = t.Result;
                return t.Result;
            });
        }

        public Task<double> RunTest()
        {
            return Task.Run(() => Trainer.RunTest()).ContinueWith(t =>
            {
                if (Report != null) Report.TestError = t.Result;
                return t.Result;
            });
        }

        public void ResetParameters()
        {
            Network.RebuildMatrices();
            _reseted = true;
        }
    }
}