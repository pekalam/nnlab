using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Prism.Events;
using Prism.Regions;
using Shell.Application.Controllers;
using Shell.Interface;

namespace Shell.Application
{
    class ModuleController
    {
        private AppState _appState;
        private StatusBarController _statusBarController;
        private IEventAggregator _ea;


        public ModuleController(IRegionManager rm, StatusBarController statusBarController, AppState appState, IEventAggregator ea)
        {
            _statusBarController = statusBarController;
            _appState = appState;
            _ea = ea;
            ActionMenuHelpers.RegionManager = rm;

        }

        public void Run()
        {
            _statusBarController.Initialize();
        }
    }
}
