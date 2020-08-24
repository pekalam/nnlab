using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using Common.Domain;
using Common.Framework;

namespace Training.Application.ViewModels
{
    public class TrainingInfoViewModel : ViewModelBase<TrainingInfoViewModel>
    {
        private TrainingParameters _trainingParameters;
        private DateTime? _startTime;
        private int _iterationsPerEpoch;
        private Timer _timer = new Timer(1000);
        private DateTime _timerDate;
        private double? _validationError;
        private double? _testError;

        public TrainingInfoViewModel()
        {
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTimer(DateTime.Now - _timerDate);
        }

        internal Action<TimeSpan> UpdateTimer { get; set; }
        internal Action<double, int, int> UpdateTraining { get; set; }


        public void StartTimer()
        {
            _timerDate = DateTime.Now;
            UpdateTimer(TimeSpan.Zero);
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        public TrainingParameters TrainingParameters
        {
            get => _trainingParameters;
            set => SetProperty(ref _trainingParameters, value);
        }

        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public int IterationsPerEpoch
        {
            get => _iterationsPerEpoch;
            set => SetProperty(ref _iterationsPerEpoch, value);
        }

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
    }
}
