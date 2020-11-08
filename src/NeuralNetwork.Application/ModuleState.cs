using Common.Domain;
using NNLibAdapter;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NeuralNetwork.Application
{
    public class ModuleState : BindableBase
    {
        private readonly Dictionary<Session, NNLibModelAdapter> _sessionToAdapter = new Dictionary<Session, NNLibModelAdapter>();
        private readonly AppState _appState;

        public event Action<NNLibModelAdapter>? NetworkStructureChanged; 

        public ModuleState(AppState appState)
        {
            _appState = appState;
            _appState.ActiveSessionChanged += (_, args) =>
            {
                if (args.prev != null && _sessionToAdapter.ContainsKey(args.next))
                {
                    RaisePropertyChanged(nameof(ModelAdapter));
                }
            };
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

        public void SetupActiveSession()
        {
            var session = _appState.ActiveSession;
            Debug.Assert(session?.Network != null);

            var adapter = new NNLibModelAdapter();
            adapter.SetNeuralNetwork(session.Network);
            adapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";
            _sessionToAdapter[session] = adapter;

            RaisePropertyChanged(nameof(ModelAdapter));
        }

        internal void AdjustNetworkLabels(TrainingData data)
        {
            ModelAdapter!.SetInputLabels(data.Variables.InputVariableNames);
            ModelAdapter.SetOutputLabels(data.Variables.TargetVariableNames);
        }

        public void RaiseNetworkStructureChanged()
        {
            Debug.Assert(ModelAdapter != null, nameof(ModelAdapter) + " != null");
            NetworkStructureChanged?.Invoke(ModelAdapter);
        }
    }
}
