using System;
using System.Collections.Generic;
using System.Text;
using Prism.Regions;
using Shell.Application.Controllers;
using Shell.Interface;

namespace Shell.Application
{
    class ModuleController
    {
        private StatusBarController _statusBarController;

        public ModuleController(IRegionManager rm, StatusBarController statusBarController)
        {
            _statusBarController = statusBarController;
            ActionMenuHelpers.RegionManager = rm;
        }

        public void Run()
        {
            _statusBarController.Initialize();
        }
    }
}
