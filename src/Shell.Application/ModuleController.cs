using Common.Domain;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;

namespace Shell.Application
{
    class ModuleController
    {
        private AppState _appState;
        private IEventAggregator _ea;


        public ModuleController(IRegionManager rm, AppState appState, IEventAggregator ea)
        {
            _appState = appState;
            _ea = ea;
            ActionMenuHelpers.RegionManager = rm;

        }

        public void Run()
        {
        }
    }
}
