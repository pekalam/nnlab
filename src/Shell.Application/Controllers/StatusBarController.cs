using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Diagnostics.Tracing;
using System.Text;
using System.Windows;
using Common.Domain;
using Common.Framework;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Shell.Application.Controllers;
using Shell.Application.Services;
using Shell.Application.ViewModels;
using Shell.Interface;
using Training.Interface;

namespace Shell.Application.Services
{
    public interface IStatusBarService : IService
    {
        DelegateCommand AddSessionCommand { get; set; }
        DelegateCommand DuplicateSessionCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<StatusBarController>().RegisterSingleton<IStatusBarService, StatusBarController>();
        }
    }
}

namespace Shell.Application.Controllers
{
    internal class StatusBarController : ISingletonController,IStatusBarService
    {
        private readonly AppState _appState;

        public StatusBarController(IEventAggregator ea, AppState appState)
        {
            _appState = appState;
            ea.GetEvent<ShowErrorNotification>().Subscribe(OnShowErrorNotification);
            ea.GetEvent<HideErrorNotification>().Subscribe(OnHideErrorNotification);

            ea.GetEvent<TrainingSessionStarted>().Subscribe(TrainingStarted, ThreadOption.UIThread);
            ea.GetEvent<TrainingSessionStopped>().Subscribe(TrainingStopped, ThreadOption.UIThread);
            ea.GetEvent<TrainingSessionPaused>().Subscribe(TrainingPaused, ThreadOption.UIThread);

            AddSessionCommand = new DelegateCommand(AddSession);
            DuplicateSessionCommand = new DelegateCommand(DuplicateSession);
        }

        public DelegateCommand AddSessionCommand { get; set; }
        public DelegateCommand DuplicateSessionCommand { get; set; }

        public void Initialize()
        {
        }

        private void DuplicateSession()
        {
            
        }

        private void AddSession()
        {

        }


        private void TrainingPaused()
        {
            var vm = StatusBarViewModel.Instance;

        }

        private void TrainingStopped()
        {
            var vm = StatusBarViewModel.Instance;

        }

        private void TrainingStarted()
        {
            var vm = StatusBarViewModel.Instance;

        }

        private void OnHideErrorNotification()
        {
            var vm = StatusBarViewModel.Instance;
            vm.ErrorNotificationVisibility = Visibility.Collapsed;
        }

        private void OnShowErrorNotification(ErrorNotificationArgs args)
        {
            var vm = StatusBarViewModel.Instance;

            vm.ErrorNotificationVisibility = Visibility.Visible;
            vm.ErrorMessage = args.Message;
        }
    }
}
