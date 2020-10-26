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
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private readonly INeuralNetworkService _networkService;
        private readonly NeuralNetworkShellController _shellController;
        private NetDisplayController _netDisplayController;
        private readonly AppState _appState;
        private readonly AppStateHelper _helper;
        private bool _firstNav;
        private ModuleState _moduleState;

        public ModuleController(IEventAggregator ea, IRegionManager rm, NeuralNetworkShellController shellController, AppState appState, INeuralNetworkService networkService, NetDisplayController netDisplayController, ModuleState moduleState)
        {
            _ea = ea;
            _rm = rm;
            _shellController = shellController;
            _appState = appState;
            _networkService = networkService;
            _netDisplayController = netDisplayController;
            _moduleState = moduleState;
            _helper = new AppStateHelper(appState);

            _ea.GetEvent<SetupNewNavigationForSession>().Subscribe(OnSetupNewNavigationForSession);
            _ea.GetEvent<ReloadContentForSession>().Subscribe(OnReloadContentForSession);
        }

        public void Run()
        {
            _helper.OnTrainingDataInSession(data =>
            {
                if (data == null)
                {
                    _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);
                    return;
                }

                if (_appState.ActiveSession!.Network == null)
                {
                    CreateNetworkForSession(_appState.ActiveSession);
                }
                _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);
            });

            _helper.OnTrainingDataPropertyChanged(AdjustNetworkToNewVariables, s => s == nameof(TrainingData.Variables));

            _shellController.Initialize();


            _ea.OnFirstNavigation(ModuleIds.NeuralNetwork, () =>
            {
                _firstNav = true;
                _rm.NavigateContentRegion("NeuralNetworkShellView");
            });
        }



        private void CreateNetworkForSession(Session session)
        {
            var network = _networkService.CreateNeuralNetwork(session.TrainingData!);
            session.Network = network;
        }

        private void AdjustNetworkToNewVariables(TrainingData trainingData)
        {
            _networkService.AdjustNetworkToData(trainingData);
            _moduleState.ModelAdapter!.SetInputLabels(trainingData.Variables.InputVariableNames);
            _moduleState.ModelAdapter.SetOutputLabels(trainingData.Variables.TargetVariableNames);
        }

        private void OnReloadContentForSession((int moduleId, Session prev, Session next) arg)
        {
            if (arg.moduleId == ModuleIds.NeuralNetwork)
            {
                //null data - go to data module in order to select data source
                if (arg.next.TrainingData == null)
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
                _ea.OnFirstNavigation(ModuleIds.NeuralNetwork, () => 
                    _rm.NavigateContentRegion("NeuralNetworkShellView"));
            }
        }
    }
}
