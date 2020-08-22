using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Common.Domain;
using NNLibAdapter;
using Prism.Mvvm;

namespace NeuralNetwork.Application
{
    internal class ModuleState : BindableBase
    {
        private Dictionary<Session, NNLibModelAdapter> _sessionToAdapter = new Dictionary<Session, NNLibModelAdapter>();
        private AppState _appState;

        public ModuleState(AppState appState)
        {
            _appState = appState;

            _appState.PropertyChanged += AppStateOnPropertyChanged;
            if (_appState.ActiveSession != null) _appState.ActiveSession.PropertyChanged += ActiveSessionOnPropertyChanged;
        }

        public NNLibModelAdapter? ModelAdapter
        {
            get
            {
                if (_appState.ActiveSession == null) return null;
                if (_sessionToAdapter.ContainsKey(_appState.ActiveSession))
                {
                    return _sessionToAdapter[_appState.ActiveSession];
                }

                return null;
            }
        }

        private void AppStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.ActiveSession))
            {
                _appState.ActiveSession.PropertyChanged += ActiveSessionOnPropertyChanged;
            }
        }

        private void ActiveSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.Network))
            {
                var session = sender as Session;

                if (session!.Network != null)
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
}
