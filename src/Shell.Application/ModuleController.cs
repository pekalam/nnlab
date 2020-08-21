using System;
using System.Collections.Generic;
using System.Text;
using Prism.Regions;
using Shell.Interface;

namespace Shell.Application
{
    class ModuleController
    {
        public ModuleController(IRegionManager rm)
        {
            ActionMenuHelpers.RegionManager = rm;
        }

        public void Run() { }
    }
}
