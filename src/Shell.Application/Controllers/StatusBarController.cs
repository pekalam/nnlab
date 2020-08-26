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
using Prism.Services.Dialogs;
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
    internal class StatusBarController : IStatusBarService, ISingletonController
    {
        private readonly AppState _appState;
        private readonly IDialogService _dialogService;

        public StatusBarController(IEventAggregator ea, AppState appState, IDialogService dialogService)
        {
            _appState = appState;
            _dialogService = dialogService;
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
            _dialogService.Show("DuplicateSessionDialogView", new DialogParameters(), result =>
            {
                if (result.Result == ButtonResult.OK)
                {
                    _appState.DuplicateActiveSession(result.Parameters.GetValue<DuplicateOptions>("options"));
                }
            });
        }

        private void AddSession()
        {

        }


        private void TrainingPaused()
        {
            var vm = StatusBarViewModel.Instance;
            vm.CanModifyActiveSession = true;

        }

        private void TrainingStopped()
        {
            var vm = StatusBarViewModel.Instance;
            vm.CanModifyActiveSession = true;

        }

        private void TrainingStarted()
        {
            var vm = StatusBarViewModel.Instance;
            vm.CanModifyActiveSession = false;

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
