using System;
using System.Collections.Generic;
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

    //TODO training
    // public class SessionEndTypeConverter : IValueConverter
    // {
    //     public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //     {
    //         var t = (TrainingSessionReport) value;
    //
    //         if (t == null)
    //         {
    //             return "";
    //         }
    //
    //         switch (t.SessionEndType)
    //         {
    //             case SessionEndType.TargetReached:
    //                 return $"Target reached ({t.TrainingParameters.TargetError})";
    //
    //             case SessionEndType.Stopped:
    //                 return "Session stopped";
    //             case SessionEndType.Timeout:
    //                 return "Timeout";
    //             case SessionEndType.MaxEpoch:
    //                 return "Max epoch number reached";
    //             case SessionEndType.NaNResult:
    //                 return "Error reached NaN value";
    //             case SessionEndType.AlgorithmError:
    //                 return "Algorithm error";
    //             case SessionEndType.Paused:
    //                 return "Session paused";
    //         }
    //
    //         return string.Empty;
    //     }
    //
    //     public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

    public class TrainingSessionReport
    {
        public SessionEndType SessionEndType { get; }
        public int TotalEpochs { get; }
        public double Error { get; }
        public TrainingParameters TrainingParameters { get; }
        public DateTime StartDate { get; }
        public TimeSpan EndTime { get; }
        public string SessionName { get; }
        public double? ValidationError { get; set; }
        public double? TestError { get; set; }

        public TrainingSessionReport(SessionEndType sessionEndType, int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startDate, TimeSpan endTime, string sessionName)
        {
            SessionEndType = sessionEndType;
            TotalEpochs = totalEpochs;
            Error = error;
            TrainingParameters = trainingParameters;
            StartDate = startDate;
            EndTime = endTime;
            SessionName = sessionName;
        }

        public static TrainingSessionReport CreateStoppedSessionReport(int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startTime, string sessionName) =>
            new TrainingSessionReport(SessionEndType.Stopped, totalEpochs, error, trainingParameters, startTime, DateTime.Now - startTime, sessionName);

        public static TrainingSessionReport CreatePausedSessionReport(int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startTime, string sessionName) =>
            new TrainingSessionReport(SessionEndType.Paused, totalEpochs, error, trainingParameters, startTime, DateTime.Now - startTime, sessionName);

        public static TrainingSessionReport CreateTargetReachedSessionReport(int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startTime, string sessionName) =>
            new TrainingSessionReport(SessionEndType.TargetReached, totalEpochs, error, trainingParameters, startTime, DateTime.Now - startTime, sessionName);

        public static TrainingSessionReport CreateMaxEpochSessionReport(int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startTime, string sessionName) =>
            new TrainingSessionReport(SessionEndType.MaxEpoch, totalEpochs, error, trainingParameters, startTime, DateTime.Now - startTime, sessionName);

        public static TrainingSessionReport CreateTimeoutSessionReport(int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startTime, string sessionName) =>
            new TrainingSessionReport(SessionEndType.Timeout, totalEpochs, error, trainingParameters, startTime, DateTime.Now - startTime, sessionName);

        public static TrainingSessionReport CreateNaNSessionReport(int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startTime, string sessionName) =>
            new TrainingSessionReport(SessionEndType.NaNResult, totalEpochs, error, trainingParameters, startTime, DateTime.Now - startTime, sessionName);

        public static TrainingSessionReport CreateAlgorithmErrorSessionReport(int totalEpochs, double error,
            TrainingParameters trainingParameters, DateTime startTime, string sessionName) =>
            new TrainingSessionReport(SessionEndType.AlgorithmError, totalEpochs, error, trainingParameters, startTime, DateTime.Now - startTime, sessionName);
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
                Report = TrainingSessionReport.CreateStoppedSessionReport(Trainer.Epochs, Trainer.Error, TrainingParameters, StartTime, SessionName);
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
                    return TrainingSessionReport.CreateStoppedSessionReport(Trainer.Epochs, Trainer.Error,
                        TrainingParameters, StartTime, SessionName);
                }

                if ((DateTime.Now - StartTime) + _elapsed > TrainingParameters.MaxLearningTime)
                {
                    Stopped = true;
                    return TrainingSessionReport.CreateTimeoutSessionReport(Trainer.Epochs, Trainer.Error,
                        TrainingParameters, StartTime, SessionName);
                }

                return TrainingSessionReport.CreatePausedSessionReport(Trainer.Epochs, Trainer.Error,
                    TrainingParameters, StartTime, SessionName);
            }

            CheckCanStart();

            if (double.IsNaN(Trainer.Error) && !_reseted)
            {
                return TrainingSessionReport.CreateNaNSessionReport(Trainer.Epochs, Trainer.Error, TrainingParameters, StartTime, SessionName);
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
                    return TrainingSessionReport.CreateAlgorithmErrorSessionReport(Trainer.Epochs,error,TrainingParameters, StartTime, SessionName);
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
                    return TrainingSessionReport.CreateMaxEpochSessionReport(Trainer.Epochs, error, TrainingParameters, StartTime, SessionName);
                }

                if (double.IsNaN(error))
                {
                    return TrainingSessionReport.CreateNaNSessionReport(Trainer.Epochs, error, TrainingParameters, StartTime, SessionName);
                }

            } while (error > TrainingParameters.TargetError);

            Stopped = true;
            return TrainingSessionReport.CreateTargetReachedSessionReport(Trainer.Epochs, error, TrainingParameters, StartTime, SessionName);
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