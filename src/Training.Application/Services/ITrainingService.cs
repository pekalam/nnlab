using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Common.Framework;
using NNLib;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.ViewModels;
using Training.Application.ViewModels.PanelLayout;
using Training.Domain;

namespace Training.Application.Services
{
    public class ModuleStateSessionOptionsDecorator : BindableBase
    {
        private ModuleStateHelper _moduleStateHelper;
        private TrainingSession _session;

        public ModuleStateSessionOptionsDecorator(ModuleStateHelper moduleStateHelper)
        {
            _moduleStateHelper = moduleStateHelper;

            _moduleStateHelper.OnActiveSessionChanged(session => Session = session);
            ResetSessionCommand = new DelegateCommand(ResetSession);
        }

        public ICommand ResetSessionCommand { get; }

        public TrainingSession Session
        {
            get => _session;
            set => SetProperty(ref _session, value);
        }

        private async void ResetSession()
        {
            await _session.ResetSession();
        }
    }

    public interface ITrainingService
    {
        DelegateCommand StartTrainingSessionCommand { get; }
        DelegateCommand StopTrainingSessionCommand { get; }
        DelegateCommand PauseTrainingSessionCommand { get; }

        DelegateCommand OpenReportsCommand { get;  }
        DelegateCommand OpenParametersCommand { get; }

        DelegateCommand SelectPanelsClickCommand { get; }

        DelegateCommand ResetParametersCommand { get; }


        ModuleStateSessionOptionsDecorator SessionOptionsDecorator { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<TrainingService>().RegisterSingleton<TrainingController>().RegisterSingleton<ITrainingService, TrainingService>();
        }
    }

    public class TrainingService : ITrainingService
    {
        private readonly IViewModelAccessor _accessor;
        private readonly IRegionManager _rm;

        public TrainingService(IViewModelAccessor accessor, ModuleStateSessionOptionsDecorator sessionOptionsDecorator, IRegionManager rm)
        {
            _accessor = accessor;
            SessionOptionsDecorator = sessionOptionsDecorator;
            _rm = rm;
        }

        public DelegateCommand StartTrainingSessionCommand { get; set; } = null!;
        public DelegateCommand StopTrainingSessionCommand { get; set; } = null!;
        public DelegateCommand PauseTrainingSessionCommand { get; set; } = null!;
        public DelegateCommand OpenReportsCommand { get; set; } = null!;
        public DelegateCommand OpenParametersCommand { get; set; } = null!;
        public DelegateCommand SelectPanelsClickCommand { get; set; } = null!;
        public DelegateCommand ResetParametersCommand { get; set; } = null!;
        public ModuleStateSessionOptionsDecorator SessionOptionsDecorator { get; }

        public void ShowPanels(PanelLayoutNavigationParams navParams)
        {
            _rm.Regions[TrainingViewRegions.PanelLayoutRegion].RequestNavigate("PanelLayoutView", navParams);
            var vm = _accessor.Get<TrainingViewModel>()!;
            vm.SelectPanelsButtonVisibility = Visibility.Collapsed;
            vm.PanelsContainerVisibility = vm.UpperSelectPanelsButtonVisibility = Visibility.Visible;
        }

        public void HidePanels()
        {
            var vm = _accessor.Get<TrainingViewModel>()!;
            vm.SelectPanelsButtonVisibility = Visibility.Visible;
            vm.PanelsContainerVisibility = Visibility.Hidden;
            vm.UpperSelectPanelsButtonVisibility = Visibility.Collapsed;
        }
    }
}
