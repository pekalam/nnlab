using Common.Domain;
using NNLib.MLP;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
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


    public class ModuleStateHelper
    {
        private readonly ModuleState _moduleState;
        private EventHandler<(TrainingSession? prev, TrainingSession next)>? _activeSessionChangedHandler;
        private PropertyChangedEventHandler? _trainerChangedInSession;
        private Action<MLPTrainer>? _trainerChanged;

        public ModuleStateHelper(ModuleState moduleState)
        {
            _moduleState = moduleState;
        }

        public void OnActiveSessionChanged(Action<TrainingSession> action)
        {
            _activeSessionChangedHandler = (_, args) =>
            {
                action(args.next);
            };

            if (_moduleState.ActiveSession != null)
            {
                action(_moduleState.ActiveSession);
            }
            _moduleState.ActiveSessionChanged -= _activeSessionChangedHandler;
            _moduleState.ActiveSessionChanged += _activeSessionChangedHandler;
        }


        public void OnTrainerChanged(Action<MLPTrainer> action)
        {
            _trainerChangedInSession = (sender, args) =>
            {
                if (args.PropertyName == nameof(TrainingSession.Trainer))
                {
                    _trainerChanged!((sender as TrainingSession)!.Trainer!);
                }
            };

            _trainerChanged = action;
            OnActiveSessionChanged(session =>
            {
                if (session.Trainer != null)
                {
                    _trainerChanged(session.Trainer);
                }

                session.PropertyChanged -= _trainerChangedInSession;
                session.PropertyChanged += _trainerChangedInSession;
            });
        }
    }

    public class ModuleState : BindableBase
    {
        private readonly AppState _appState;
        private readonly Dictionary<Session, TrainingSession> _sessionToTraining = new Dictionary<Session, TrainingSession>();
        private TrainingSession? _previousActive;

        public event EventHandler<(TrainingSession? prev, TrainingSession next)>? ActiveSessionChanged; 

        public ModuleState(AppState appState)
        {
            _appState = appState;
        }

        public void CreateOrSetActiveTrainingSession()
        {
            if (_sessionToTraining.TryGetValue(_appState.ActiveSession!, out var session))
            {
                RaisePropertyChanged(nameof(ActiveSession));
                ActiveSessionChanged?.Invoke(this, (_previousActive, session));
                _previousActive = session;
            }
            else
            {
                var newSession = new TrainingSessionDecorator(_appState);
                _sessionToTraining[_appState.ActiveSession!] = newSession;
                RaisePropertyChanged(nameof(ActiveSession));
                ActiveSessionChanged?.Invoke(this, (_previousActive, newSession));
                _previousActive = newSession;
            }

        }

        public TrainingSession? ActiveSession
        {
            get
            {
                if (_appState.ActiveSession == null) return null;
                _sessionToTraining.TryGetValue(_appState.ActiveSession, out var session);
                return session;
            }
        }
    }
}