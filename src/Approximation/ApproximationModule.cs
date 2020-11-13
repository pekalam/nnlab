using Approximation.Application;
using Prism.Ioc;
using Prism.Modularity;

namespace Approximation
{
    public class ApproximationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ModuleController>().Run();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ModuleController>();

            Bootstraper.RegisterTypes(containerRegistry);
            Presentation.Bootstraper.RegisterTypes(containerRegistry);
        }
    }
}