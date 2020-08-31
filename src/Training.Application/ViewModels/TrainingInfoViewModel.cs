using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Timers;
using System.Windows;
using Common.Domain;
using Common.Framework;
using Training.Application.Views;
using Training.Domain;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingInfoViewModel : ViewModelBase<TrainingInfoViewModel, ITrainingInfoView>
    {
        private readonly Timer _timer = new Timer(1000);
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
            _timer.Elapsed += (_,__) => View!.UpdateTimer(Time.Now - _timerDate);

            if (ModuleState.ActiveSession != null)
            {
                ModuleState.ActiveSession.PropertyChanged += OnTraininerChanged;
                if (ModuleState.ActiveSession.Trainer != null)
                {
                    RaisePropertyChanged(nameof(IterationsPerEpoch));
                }
            }

            ModuleState.ActiveSessionChanged += (sender, args) => args.next.PropertyChanged += OnTraininerChanged;

        }

        private void OnTraininerChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingSession.Trainer))
            {
                RaisePropertyChanged(nameof(IterationsPerEpoch));
            }
        }

        protected override void ViewChanged(ITrainingInfoView view)
        {
            view.ResetProgress();
        }

        /// <summary>
        /// Starts view timer from 0
        /// </summary>
        public void StartTimer()
        {
            _timerDate = Time.Now;
            View!.UpdateTimer(TimeSpan.Zero);
            _timer.Start();
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        public ModuleState ModuleState { get; }
        public AppState AppState { get; }

        public int? IterationsPerEpoch => ModuleState.ActiveSession!.Trainer!.Algorithm.BatchTrainer?.IterationsPerEpoch;

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
