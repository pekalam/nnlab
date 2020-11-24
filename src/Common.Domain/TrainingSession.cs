using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NNLib.MLP;

namespace Common.Domain
{
    public class EpochEndArgs
    {
        public int Epoch;
        public double Error;
        public int Iterations;
        public double? ValidationError;
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
        ValidationErrorReached,
    }

    public enum TrainingReportAlgorithm
    {
        GradientDescentOnline,
        GradientDescentBatch,
        MomentumGradientDescentBatch,
        MomentumGradientDescentOnline,
        LevenbergMarquardt,
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

        internal void Reset()
        {
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
    
    public class TrainingSessionReport : BindableBase
    {
        private double? _validationError;
        private double? _testError;

        public static bool IsTerminatingSessionType(SessionEndType type) =>
            type == SessionEndType.Stopped || type == SessionEndType.TargetReached || type == SessionEndType.ValidationErrorReached;
        
        public SessionEndType SessionEndType { get; }
        public int TotalEpochs { get; }
        public double Error { get; }
        public DateTime StartDate { get; }
        public TimeSpan Duration { get; }
        
        public DateTime EndDate { get; }

        public TrainingReportAlgorithm Algorithm { get; }

        public double? ValidationError
        {
            get => _validationError;
            set => SetProperty(ref _validationError, value);
        }

        public double? TestError
        {
            get => _testError;
            set => SetProperty(ref _testError, value);
        }

        public EpochEndArgs[] EpochEndEventArgs { get; }

        public MLPNetwork Network { get; }

        public TrainingSessionReport(SessionEndType sessionEndType, int totalEpochs, double error, DateTime startDate, TimeSpan duration, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork network, TrainingReportAlgorithm algorithm, double? validationError = null)
        {
            SessionEndType = sessionEndType;
            TotalEpochs = totalEpochs;
            Error = error;
            StartDate = startDate;
            Duration = duration;
            Network = network.Clone();
            EndDate = startDate + duration;
            Algorithm = algorithm;
            EpochEndEventArgs = epochEndEventArgs.Select(e => new EpochEndArgs()
            {
                Epoch = e.Epoch, Error = e.Error, Iterations = e.Iterations,
            }).ToArray();
            ValidationError = validationError;
        }

        // public static TrainingSessionReport CreateStoppedSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.Stopped, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm);
        //
        // public static TrainingSessionReport CreatePausedSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.Paused, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm);
        //
        // public static TrainingSessionReport CreateTargetReachedSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.TargetReached, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm);
        //
        // public static TrainingSessionReport CreateMaxEpochSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.MaxEpoch, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm);
        //
        // public static TrainingSessionReport CreateTimeoutSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.Timeout, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm);
        //
        // public static TrainingSessionReport CreateNaNSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.NaNResult, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm);
        //
        // public static TrainingSessionReport CreateAlgorithmErrorSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.AlgorithmError, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm);
        //
        // public static TrainingSessionReport CreateValidationErrorReachedSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs, MLPNetwork net, double validationError, TrainingAlgorithm algorithm) =>
        //     new TrainingSessionReport(SessionEndType.ValidationErrorReached, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs, net, algorithm){ValidationError = validationError};
    }
}