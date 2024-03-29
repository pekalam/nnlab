﻿using Common.Domain;
using Common.Framework;
using NNLib.Training.GradientDescent;
using System;
using System.Timers;
using System.Windows.Threading;
using Training.Application.Controllers;
using Training.Application.Views;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingInfoViewModel : ViewModelBase<TrainingInfoViewModel, ITrainingInfoView>
    {
        private readonly Timer _timer = new Timer(1000);
        private DateTime _timerDate;

#pragma warning disable 8618
        public TrainingInfoViewModel()
#pragma warning restore 8618
        {
        }

        [InjectionConstructor]
        public TrainingInfoViewModel(ITrainingInfoController controller,ModuleState moduleState, AppState appState)
        {
            ModuleState = moduleState;
            AppState = appState;
            var helper = new ModuleStateHelper(moduleState);
            _timer.Elapsed += (_, __) => System.Windows.Application.Current?.Dispatcher.InvokeAsync(() => View!.UpdateTimer(Time.Now - _timerDate), DispatcherPriority.Send);

            helper.OnTrainerChanged(trainer =>
            {
                RaisePropertyChanged(nameof(IterationsPerEpoch));
                ModuleState.ActiveSession!.TrainerUpdated -= ActiveSessionOnTrainerUpdated;
                ModuleState.ActiveSession.TrainerUpdated += ActiveSessionOnTrainerUpdated;
            });

            helper.OnActiveSessionChanged(session =>
            {
                session.SessionReset -= SessionOnSessionReset;
                session.SessionReset += SessionOnSessionReset;
            });

            controller.Initialize(this);
        }

        private void SessionOnSessionReset()
        {
            View?.ResetProgress();
        }

        private void ActiveSessionOnTrainerUpdated()
        {
            RaisePropertyChanged(nameof(IterationsPerEpoch));
        }

        protected override void ViewChanged(ITrainingInfoView view)
        {
            view.ResetProgress();
        }

        public void RestartTimer()
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

        public int? IterationsPerEpoch => (ModuleState.ActiveSession!.Trainer!.Algorithm as GradientDescentAlgorithm)?.IterationsPerEpoch ?? 0;
    }
}
