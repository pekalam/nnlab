using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Common.Framework;
using Prism.Commands;
using Prism.Ioc;
using Training.Application.Controllers;
using Training.Application.ViewModels;

namespace Training.Application.Services
{
    public interface ITrainingService
    {
        DelegateCommand StartTrainingSessionCommand { get; }
        DelegateCommand StopTrainingSessionCommand { get; }
        DelegateCommand PauseTrainingSessionCommand { get; }

        DelegateCommand OpenReportsCommand { get;  }
        DelegateCommand OpenParametersCommand { get; }

        DelegateCommand SelectPanelsClickCommand { get; }

        DelegateCommand ResetParametersCommand { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<TrainingService>().RegisterSingleton<TrainingController>().RegisterSingleton<ITrainingService, TrainingService>();
        }
    }

    public class TrainingService : ITrainingService
    {
        private readonly IViewModelAccessor _accessor;

        public TrainingService(IViewModelAccessor accessor)
        {
            _accessor = accessor;
        }

        public DelegateCommand StartTrainingSessionCommand { get; set; } = null!;
        public DelegateCommand StopTrainingSessionCommand { get; set; } = null!;
        public DelegateCommand PauseTrainingSessionCommand { get; set; } = null!;
        public DelegateCommand OpenReportsCommand { get; set; } = null!;
        public DelegateCommand OpenParametersCommand { get; set; } = null!;
        public DelegateCommand SelectPanelsClickCommand { get; set; } = null!;
        public DelegateCommand ResetParametersCommand { get; set; } = null!;

        public void OnShowPanels()
        {
            var vm = _accessor.Get<TrainingViewModel>()!;
            vm.SelectPanelsButtonVisibility = Visibility.Collapsed;
            vm.PanelsContainerVisibility = vm.UpperSelectPanelsButtonVisibility = Visibility.Visible;
        }

        public void OnHidePanels()
        {
            var vm = _accessor.Get<TrainingViewModel>()!;
            vm.SelectPanelsButtonVisibility = Visibility.Visible;
            vm.PanelsContainerVisibility = Visibility.Hidden;
            vm.UpperSelectPanelsButtonVisibility = Visibility.Collapsed;
        }
    }
}
