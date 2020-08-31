using Prediction.Application;
using Prism.Ioc;
using Prism.Modularity;

namespace Prediction
{
    public class PredictionModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ModuleController>().Run();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ModuleController>();

            Prediction.Application.Bootstraper.RegisterTypes(containerRegistry);
            Prediction.Presentation.Bootstraper.RegisterTypes(containerRegistry);
        }
    }
}