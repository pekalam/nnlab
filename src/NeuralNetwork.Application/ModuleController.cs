using Common.Domain;
using NeuralNetwork.Application.ViewModels;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;

namespace NeuralNetwork.Application
{
    class ModuleController
    {
        private IEventAggregator ea;
        private IRegionManager rm;

        public ModuleController(IEventAggregator ea, IRegionManager rm)
        {
            this.ea = ea;
            this.rm = rm;
        }

        public void Run()
        {
            ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.NeuralNetwork);

            ea.OnFirstNavigation(ModuleIds.NeuralNetwork, () =>
            {
                rm.NavigateContentRegion(nameof(ViewAViewModel), "Network");
            });
        }
    }
}
