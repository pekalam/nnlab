using System.Collections.Specialized;
using System.ComponentModel;
using Common.Domain;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;

namespace NeuralNetwork.Application
{
    class ModuleController
    {
        private IEventAggregator ea;
        private IRegionManager rm;
        private INeuralNetworkService _networkService;
        private NeuralNetworkShellController _shellController;
        private NetDisplayController _netDisplayController;
        private AppState _appState;

        public ModuleController(IEventAggregator ea, IRegionManager rm, NeuralNetworkShellController shellController, AppState appState, INeuralNetworkService networkService, NetDisplayController netDisplayController)
        {
            this.ea = ea;
            this.rm = rm;
            _shellController = shellController;
            _appState = appState;
            _networkService = networkService;
            _netDisplayController = netDisplayController;
        }

        public void Run()
        {
            _appState.PropertyChanged += AppStateOnPropertyChanged;
            if (_appState.ActiveSession != null)
            {
                //TODO mv
                ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);

                if (_appState.ActiveSession.TrainingData != null)
                {
                    CreateNetworkForSession(_appState.ActiveSession);
                }
                else
                {
                    _appState.ActiveSession.PropertyChanged += ActiveSessionOnTrainingDataChanged;
                }
            }

            _shellController.Initialize();


            ea.OnFirstNavigation(ModuleIds.NeuralNetwork, () =>
            {
                rm.NavigateContentRegion(nameof(NeuralNetworkShellViewModel), "Neural network");
            });
        }

        private void AppStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AppState.ActiveSession):
                {
                    ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);

                    if (_appState.ActiveSession.TrainingData != null)
                    {
                        CreateNetworkForSession(_appState.ActiveSession);
                    }
                    else
                    {
                        _appState.ActiveSession.PropertyChanged += ActiveSessionOnTrainingDataChanged;
                    }
                    break;
                }
            }
        }

        private void ActiveSessionOnTrainingDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.TrainingData))
            {
                CreateNetworkForSession(_appState.ActiveSession);
                _appState.ActiveSession.PropertyChanged -= ActiveSessionOnTrainingDataChanged;
            }
        }

        private void CreateNetworkForSession(Session session)
        {
            var network = _networkService.CreateDefaultNetwork();
            session.Network = network;
            _networkService.AdjustParametersToTrainingData(session.TrainingData);
        }
    }
}
