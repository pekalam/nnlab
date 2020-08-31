using System.ComponentModel;
using Common.Domain;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;

namespace Prediction.Application
{
    internal class ModuleController
    {
        private IEventAggregator _ea;
        private IRegionManager _rm;
        private AppState _appState;

        public ModuleController(IEventAggregator ea, IRegionManager rm, AppState appState)
        {
            _ea = ea;
            _rm = rm;
            _appState = appState;

        }

        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) e)
        {
            SendEnabledDisabledForActiveSession();

            e.next.PropertyChanged -= ActiveSessionPropertyChanged;
            e.next.PropertyChanged += ActiveSessionPropertyChanged;
        }

        private void ActiveSessionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SendEnabledDisabledForActiveSession();
        }

        public void Run()
        {
            _appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;

            SendEnabledDisabledForActiveSession();

            _ea.OnFirstNavigation(ModuleIds.Prediction, () =>
            {
                _rm.NavigateContentRegion("PredictView", "Predict");
            });

            _ea.GetEvent<ReloadContentForSession>().Subscribe(args =>
            {
                if (args.moduleId == ModuleIds.Prediction)
                {
                    if (_appState.ActiveSession.TrainingData == null || _appState.ActiveSession.Network == null)
                    {
                        _ea.GetEvent<CheckNavMenuItem>().Publish(ModuleIds.Data);
                    }
                    else
                    {
                        _rm.NavigateContentRegion("PredictView", "Predict");
                    }

                }
            });

            _ea.GetEvent<SetupNewNavigationForSession>().Subscribe(args =>
            {
                if (args.moduleId == ModuleIds.Prediction)
                {
                    _ea.OnFirstNavigation(ModuleIds.Prediction, () =>
                    {
                        _rm.NavigateContentRegion("PredictView", "Predict");
                    });
                }
            });
        }

        private void SendEnabledDisabledForActiveSession()
        {
            if (_appState.ActiveSession?.TrainingData != null && _appState.ActiveSession.Network != null)
            {
                _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Prediction);
            }
            else
            {
                _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Prediction);
            }
        }
    }
}