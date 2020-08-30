using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Common.Domain;
using Prism.Mvvm;
using Training.Application.Plots;
using Training.Domain;

namespace Training.Application
{
    internal class TrainingSessionDecorator : TrainingSession
    {
        public TrainingSessionDecorator(AppState appState) : base(appState)
        {
        }

        protected override async Task<TrainingSessionReport> InternalStart()
        {
            var report = await base.InternalStart();
            await GlobalDistributingDispatcher.WaitForQueued();

            return report;
        }
    }

    public class ModuleState : BindableBase
    {
        private readonly AppState _appState;
        private readonly Dictionary<Session, TrainingSession> _sessionToTraining = new Dictionary<Session, TrainingSession>();
        private TrainingSession? _activeSession;

        public event EventHandler<(TrainingSession? prev, TrainingSession next)> ActiveSessionChanged; 

        public ModuleState(AppState appState)
        {
            _appState = appState;

            _appState.PropertyChanged += AppStateOnPropertyChanged;
        }

        private void AppStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.ActiveSession))
            {
                _sessionToTraining.TryGetValue(_appState.ActiveSession!, out var trainingSession);
                if (trainingSession == null)
                {
                    ActiveSession = new TrainingSessionDecorator(_appState);
                    _sessionToTraining[_appState.ActiveSession!] = _activeSession!;
                }
                else
                {
                    ActiveSession = trainingSession;
                }
            }
        }

        public TrainingSession? ActiveSession
        {
            get => _activeSession;
            set
            {
                if(value == null) throw new NullReferenceException("Null training session");
                var temp = _activeSession;
                SetProperty(ref _activeSession, value);
                ActiveSessionChanged?.Invoke(this, (temp, value));
            }
        }
    }
}