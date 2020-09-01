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
        private IEventAggregator _ea;
        private IRegionManager _rm;
        private readonly INeuralNetworkService _networkService;
        private readonly NeuralNetworkShellController _shellController;
        private NetDisplayController _netDisplayController;
        private readonly AppState _appState;
        private bool _firstNav;

        public ModuleController(IEventAggregator ea, IRegionManager rm, NeuralNetworkShellController shellController, AppState appState, INeuralNetworkService networkService, NetDisplayController netDisplayController)
        {
            _ea = ea;
            _rm = rm;
            _shellController = shellController;
            _appState = appState;
            _networkService = networkService;
            _netDisplayController = netDisplayController;

            _ea.GetEvent<SetupNewNavigationForSession>().Subscribe(OnSetupNewNavigationForSession);
            _ea.GetEvent<ReloadContentForSession>().Subscribe(OnReloadContentForSession);

        }

        public void Run()
        {
            _appState.PropertyChanged += AppStateOnPropertyChanged;
            if (_appState.ActiveSession != null)
            {
                SetupActiveSession();
            }

            _shellController.Initialize();


            _ea.OnFirstNavigation(ModuleIds.NeuralNetwork, () =>
            {
                _firstNav = true;
                _rm.NavigateContentRegion(nameof(NeuralNetworkShellViewModel), "Neural network");
            });
        }

        

        private void SetupActiveSession()
        {

            if (_appState.ActiveSession!.TrainingData != null)
            {
                _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);
                if (_appState.ActiveSession.Network == null)
                {
                    CreateNetworkForSession(_appState.ActiveSession);
                }
            }
            else
            {
                _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);
            }
            _appState.ActiveSession.PropertyChanged -= ActiveSessionOnTrainingDataChanged;
            _appState.ActiveSession.PropertyChanged += ActiveSessionOnTrainingDataChanged;
        }

        private void CreateNetworkForSession(Session session)
        {
            var network = _networkService.CreateNeuralNetwork(session.TrainingData!);
            session.Network = network;


            session.TrainingData!.PropertyChanged -= TrainingDataOnPropertyChanged;
            session.TrainingData.PropertyChanged += TrainingDataOnPropertyChanged;
        }

        private void AdjustNetworkToNewVariables(TrainingData trainingData)
        {
            _networkService.SetInputsCount(trainingData.Variables.InputVariableNames.Length);
            _networkService.SetNeuronsCount(_appState.ActiveSession!.Network!.Layers[^1],
                trainingData.Variables.TargetVariableNames.Length);
        }

        private void TrainingDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingData.Variables))
            {
                var trainingData = (sender as TrainingData)!;
                AdjustNetworkToNewVariables(trainingData);
            }
        }


        private void AppStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(AppState.ActiveSession):
                {
                    SetupActiveSession();
                    break;
                }
            }
        }

        private void OnReloadContentForSession((int moduleId, Session prev, Session next) arg)
        {
            if (arg.moduleId == ModuleIds.NeuralNetwork)
            {
                //null network - go to data module in order to select data source
                if (arg.next.Network == null)
                {
                    _ea.GetEvent<CheckNavMenuItem>().Publish(ModuleIds.Data);
                }
            }
        }

        private void OnSetupNewNavigationForSession((int moduleId, Session prev, Session next) arg)
        {
            if (arg.moduleId == ModuleIds.NeuralNetwork)
            {
                if (!_firstNav) return;
                _ea.OnFirstNavigation(ModuleIds.NeuralNetwork, () => _rm.NavigateContentRegion(nameof(NeuralNetworkShellViewModel), "Neural network"));
            }
        }

        private void ActiveSessionOnTrainingDataChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.TrainingData))
            {
                if (_appState.ActiveSession!.TrainingData != null)
                {
                    _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);
                    CreateNetworkForSession(_appState.ActiveSession);
                }
                else
                {
                    _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);
                }
            }
        }
    }
}
