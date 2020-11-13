using Common.Domain;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;

namespace Approximation.Application
{
    internal class ModuleController
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private readonly AppState _appState;
        private readonly AppStateHelper _helper;

        public ModuleController(IEventAggregator ea, IRegionManager rm, AppState appState)
        {
            _ea = ea;
            _rm = rm;
            _appState = appState;
            _helper = new AppStateHelper(appState);
        }

        public void Run()
        {
            _helper.OnNetworkInSession(network =>
            {
                SendEnabledDisabledForActiveSession();
            });

            _helper.OnTrainingDataInSession(data => SendEnabledDisabledForActiveSession());

            _ea.OnFirstNavigation(ModuleIds.Approximation, () =>
            {
                _rm.NavigateContentRegion("ApproximationView");
            });

            _ea.GetEvent<ReloadContentForSession>().Subscribe(args =>
            {
                if (args.moduleId == ModuleIds.Approximation)
                {
                    if (_appState.ActiveSession!.TrainingData == null || _appState.ActiveSession.Network == null)
                    {
                        _ea.GetEvent<CheckNavMenuItem>().Publish(ModuleIds.Data);
                    }
                    else
                    {
                        _rm.NavigateContentRegion("ApproximationView");
                    }

                }
            });

            _ea.GetEvent<SetupNewNavigationForSession>().Subscribe(args =>
            {
                if (args.moduleId == ModuleIds.Approximation)
                {
                    _ea.OnFirstNavigation(ModuleIds.Approximation, () =>
                    {
                        _rm.NavigateContentRegion("ApproximationView");
                    });
                }
            });
        }

        private void SendEnabledDisabledForActiveSession()
        {
            if (_appState.ActiveSession?.TrainingData != null && _appState.ActiveSession.Network != null)
            {
                _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Approximation);
            }
            else
            {
                _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Approximation);
            }
        }
    }
}