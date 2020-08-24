using System.ComponentModel;
using Common.Domain;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using Training.Application.Controllers;
using Training.Application.ViewModels;

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

        private ITrainingInfoController _trainingInfoController;
        private ITrainingController _trainingController;

        public ModuleController(IEventAggregator ea, IRegionManager rm, AppState appState, ITrainingInfoController trainingInfoController, ITrainingController trainingController)
        {
            _ea = ea;
            _rm = rm;
            _appState = appState;
            _trainingInfoController = trainingInfoController;
            _trainingController = trainingController;

            appState.PropertyChanged += AppStateOnPropertyChanged;
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
            var session = sender as Session;
            if (session.Network != null && session.TrainingData != null)
            {
                _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Training);
            }
        }

        public void Run()
        {

            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Training);


            _ea.OnFirstNavigation(ModuleIds.Training, () =>
            {
                _rm.NavigateContentRegion("TrainingView", "Training");
            });
        }
    }
}
