using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Windows;
using Common.Domain;
using Common.Framework;
using Training.Domain;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingInfoViewModel : ViewModelBase<TrainingInfoViewModel>
    {
        private int _iterationsPerEpoch;
        private Timer _timer = new Timer(1000);
        private DateTime _timerDate;
        private double? _validationError;
        private double? _testError;

        public TrainingInfoViewModel()
        {
        }

        [InjectionConstructor]
        public TrainingInfoViewModel(ModuleState moduleState, AppState appState)
        {
            ModuleState = moduleState;
            AppState = appState;
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            UpdateTimer(DateTime.Now - _timerDate);
        }

        //TODO view abstr

        public Action<TimeSpan> UpdateTimer { get; set; }
        public Action<double, int, int> UpdateTraining { get; set; }


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

        public ModuleState ModuleState { get; }
        public AppState AppState { get; }

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
