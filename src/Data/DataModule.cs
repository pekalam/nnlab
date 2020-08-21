using Data.Application;
using Prism.Ioc;
using Prism.Modularity;

namespace Data
{
    public class DataModule : IModule
    {
        public const int NavIdentifier = 1;

        public void OnInitialized(IContainerProvider cp)
        {
            cp.Resolve<ModuleController>().Run();
        }

        public void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ModuleController>();

            Data.Domain.Bootstraper.RegisterTypes(cr);
            Data.Application.Bootstraper.RegisterTypes(cr);
            Data.Presentation.Bootstraper.RegisterTypes(cr);
        }
    }
}