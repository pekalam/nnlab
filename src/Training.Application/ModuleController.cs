using System.ComponentModel;
using Common.Domain;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using Training.Application.Controllers;
using Training.Application.ViewModels;
using Training.Domain;
using Training.Interface;

namespace Training.Application
{
    public class TrainingViewRegions
    {
        public const string PanelLayoutRegion = nameof(PanelLayoutRegion);
    }

    public enum Panels
    {
        LiveEditPanel,
        NetworkError,
        Accuracy,
        NetworkVisualization,
        MatrixPreview
    }

    public class PanelSelectModel
    {
        public Panels PanelType { get; set; }
    }

    class ModuleController
    {
        private IEventAggregator _ea;
        private IRegionManager _rm;
        private AppState _appState;
        private ModuleState _moduleState;

        private ITrainingInfoController _trainingInfoController;
        private ITrainingController _trainingController;

        public ModuleController(IEventAggregator ea, IRegionManager rm, AppState appState, ITrainingInfoController trainingInfoController, ITrainingController trainingController, ModuleState moduleState)
        {
            _ea = ea;
            _rm = rm;
            _appState = appState;
            _trainingInfoController = trainingInfoController;
            _trainingController = trainingController;
            _moduleState = moduleState;

            _moduleState.PropertyChanged += ModuleStateOnPropertyChanged;
        }

        private void SendNavMenuItemEvents()
        {
            if (_moduleState.ActiveSession.IsValid)
            {
                _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Training);
            }
            else
            {
                _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Training);
            }
        }

        private void ModuleStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ModuleState.ActiveSession))
            {
                SendNavMenuItemEvents();
                _moduleState.ActiveSession.PropertyChanged -= ActiveTrainingSessionOnPropertyChanged;
                _moduleState.ActiveSession.PropertyChanged += ActiveTrainingSessionOnPropertyChanged;
            }
        }

        private void ActiveTrainingSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var session = sender as TrainingSession;
            switch (e.PropertyName)
            {
                case nameof(TrainingSession.Started):
                    if (session.Started)
                    {
                        _ea.GetEvent<TrainingSessionStarted>().Publish();
                    }
                    break;
                case nameof(TrainingSession.Paused):
                    if (session.Paused)
                    {
                        _ea.GetEvent<TrainingSessionPaused>().Publish();
                    }
                    break;
                case nameof(TrainingSession.Stopped):
                    if (session.Stopped)
                    {
                        _ea.GetEvent<TrainingSessionStopped>().Publish();
                    }
                    break;
                case nameof(TrainingSession.IsValid):
                    SendNavMenuItemEvents();
                    break;
            }

        }


        public void Run()
        {
            _ea.OnFirstNavigation(ModuleIds.Training, () =>
            {
                _rm.NavigateContentRegion("TrainingView", "Training");
            });
        }
    }
}
