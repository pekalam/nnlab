using NeuralNetwork.Views;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Shell.Interface;

namespace NeuralNetwork
{
    public class NeuralNetworkModuleController
    {
        private IEventAggregator ea;
        private IRegionManager rm;

        public NeuralNetworkModuleController(IEventAggregator ea, IRegionManager rm)
        {
            this.ea = ea;
            this.rm = rm;
        }

        public void Run()
        {
            ea.GetEvent<EnableNavMenuItem>().Publish(NeuralNetworkModule.NavIdentifier);

            ea.OnFirstNavigation(NeuralNetworkModule.NavIdentifier, () =>
            {
                rm.NavigateContentRegion(nameof(ViewA), "Network");
            });
        }
    }

    public class NeuralNetworkModule : IModule
    {
        public const int NavIdentifier = 2;

        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<NeuralNetworkModuleController>().Run();   
        }


        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<NeuralNetworkModuleController>();
            containerRegistry.RegisterForNavigation<ViewA>();
        }
    }
}