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
using Prism.Mvvm;

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
    
    public class TrainingSessionReport : BindableBase
    {
        private double? _validationError;
        private double? _testError;

        public static bool IsTerminatingSessionType(SessionEndType type) =>
            type == SessionEndType.Stopped || type == SessionEndType.TargetReached;
        
        public SessionEndType SessionEndType { get; }
        public int TotalEpochs { get; }
        public double Error { get; }
        public DateTime StartDate { get; }
        public TimeSpan Duration { get; }
        
        public DateTime EndDate { get; }

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

        public TrainingSessionReport(SessionEndType sessionEndType, int totalEpochs, double error, DateTime startDate, TimeSpan duration, IEnumerable<EpochEndArgs> epochEndEventArgs)
        {
            if(totalEpochs < 0) throw new ArgumentException("totalEpochs < 0");
            if(error < 0d) throw new ArgumentException("error < 0");
            SessionEndType = sessionEndType;
            TotalEpochs = totalEpochs;
            Error = error;
            StartDate = startDate;
            Duration = duration;
            EndDate = startDate + duration;
            EpochEndEventArgs = epochEndEventArgs.Select(e => new EpochEndArgs()
            {
                Epoch = e.Epoch, Error = e.Error, Iterations = e.Iterations,
            }).ToArray();
        }

        public static TrainingSessionReport CreateStoppedSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs) =>
            new TrainingSessionReport(SessionEndType.Stopped, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs);

        public static TrainingSessionReport CreatePausedSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs) =>
            new TrainingSessionReport(SessionEndType.Paused, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs);

        public static TrainingSessionReport CreateTargetReachedSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs) =>
            new TrainingSessionReport(SessionEndType.TargetReached, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs);

        public static TrainingSessionReport CreateMaxEpochSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs) =>
            new TrainingSessionReport(SessionEndType.MaxEpoch, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs);

        public static TrainingSessionReport CreateTimeoutSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs) =>
            new TrainingSessionReport(SessionEndType.Timeout, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs);

        public static TrainingSessionReport CreateNaNSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs) =>
            new TrainingSessionReport(SessionEndType.NaNResult, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs);

        public static TrainingSessionReport CreateAlgorithmErrorSessionReport(int totalEpochs, double error, DateTime startTime, IEnumerable<EpochEndArgs> epochEndEventArgs) =>
            new TrainingSessionReport(SessionEndType.AlgorithmError, totalEpochs, error, startTime, Time.Now - startTime, epochEndEventArgs);
    }
}