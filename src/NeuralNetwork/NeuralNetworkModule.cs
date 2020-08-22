using NeuralNetwork.Application;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Shell.Interface;

namespace NeuralNetwork
{
    public class NeuralNetworkModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ModuleController>().Run();   
        }


        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ModuleController>();

            NeuralNetwork.Domain.Bootstraper.RegisterTypes(containerRegistry);
            NeuralNetwork.Application.Bootstraper.RegisterTypes(containerRegistry);
            NeuralNetwork.Presentation.Bootstraper.RegisterTypes(containerRegistry);
        }
    }
}