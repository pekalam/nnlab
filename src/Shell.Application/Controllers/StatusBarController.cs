using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
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
        private readonly IEventAggregator _ea;
        private readonly AppState _appState;
        
        public StatusBarController(IEventAggregator ea, AppState appState)
        {
            _ea = ea;
            _appState = appState;
            _ea.GetEvent<ShowErrorNotification>().Subscribe(OnShowErrorNotification);
            _ea.GetEvent<HideErrorNotification>().Subscribe(OnHideErrorNotification);

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
