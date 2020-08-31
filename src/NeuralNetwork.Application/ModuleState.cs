using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Common.Domain;
using NNLibAdapter;
using Prism.Mvvm;

namespace NeuralNetwork.Application
{
    public class ModuleState : BindableBase
    {
        private readonly Dictionary<Session, NNLibModelAdapter> _sessionToAdapter = new Dictionary<Session, NNLibModelAdapter>();
        private readonly AppState _appState;

        public ModuleState(AppState appState)
        {
            _appState = appState;

            if (_appState.ActiveSession != null)
            {
                SetupActiveSession(_appState.ActiveSession);
            }
            _appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;
        }

        public NNLibModelAdapter? ModelAdapter
        {
            get
            {
                if (_appState.ActiveSession?.Network == null) return null;
                if (_sessionToAdapter.ContainsKey(_appState.ActiveSession))
                {
                    return _sessionToAdapter[_appState.ActiveSession];
                }
                else
                {
                    SetupActiveSession(_appState.ActiveSession);
                    return _sessionToAdapter[_appState.ActiveSession];
                }
            }
        }



        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) e) => SetupActiveSession(e.next);

        private void ActiveSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.Network))
            {
                var session = sender as Session;
                SetAdapterIfNetworkNotNull(session!);
            }
        }

        private void SetupActiveSession(Session session)
        {
            SetAdapterIfNetworkNotNull(session);
            session.PropertyChanged += ActiveSessionOnPropertyChanged;
        }

        private void SetAdapterIfNetworkNotNull(Session session)
        {
            if (session.Network != null)
            {
                var adapter = new NNLibModelAdapter();
                adapter.SetNeuralNetwork(session.Network);
                adapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";
                _sessionToAdapter[session] = adapter;
                RaisePropertyChanged(nameof(ModelAdapter));
            }
        }
    }
}
