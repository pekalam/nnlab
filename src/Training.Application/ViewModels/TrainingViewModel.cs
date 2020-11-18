using System.Linq;
using Common.Framework;
using Prism.Regions;
using System.Windows;
using Training.Application.Controllers;
using Training.Application.ViewModels.PanelLayout;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingViewModel : ViewModelBase<TrainingViewModel>
    {
        private Visibility _selectPanelsButtonVisibility = Visibility.Visible;
        private Visibility _panelsContainerVisibility = Visibility.Hidden;
        private Visibility _upperSelectPanelsButtonVisibility = Visibility.Collapsed;
        private IRegionManager _rm;
        
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public TrainingViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        [InjectionConstructor]
        public TrainingViewModel(ITrainingController service, ModuleState moduleState, IRegionManager rm)
        {
            Service = service;
            ModuleState = moduleState;
            _rm = rm;
            KeepAlive = true;

            service.Initialize(this);
        }

        public ModuleState ModuleState { get; }

        public ITrainingController Service { get; }

        public Visibility SelectPanelsButtonVisibility
        {
            get => _selectPanelsButtonVisibility;
            set => SetProperty(ref _selectPanelsButtonVisibility, value);
        }

        public Visibility PanelsContainerVisibility
        {
            get => _panelsContainerVisibility;
            set => SetProperty(ref _panelsContainerVisibility, value);
        }

        public Visibility UpperSelectPanelsButtonVisibility
        {
            get => _upperSelectPanelsButtonVisibility;
            set => SetProperty(ref _upperSelectPanelsButtonVisibility, value);
        }


        public void ShowPanels(PanelLayoutNavigationParams navParams)
        {
            _rm.Regions[TrainingViewRegions.PanelLayoutRegion].RequestNavigate("PanelLayoutView", navParams);
            SelectPanelsButtonVisibility = Visibility.Collapsed;
            PanelsContainerVisibility = UpperSelectPanelsButtonVisibility = Visibility.Visible;
        }

        public void HidePanels()
        {
            if (_rm.Regions.ContainsRegionWithName(PanelLayoutRegions.PanelLayoutMain))
            {
                var panelLayoutMain = _rm.Regions[PanelLayoutRegions.PanelLayoutMain];
                panelLayoutMain.Deactivate(panelLayoutMain.ActiveViews.First());
                panelLayoutMain.RemoveAll();
            }

            SelectPanelsButtonVisibility = Visibility.Visible;
            PanelsContainerVisibility = Visibility.Hidden;
            UpperSelectPanelsButtonVisibility = Visibility.Collapsed;
        }
    }
}
