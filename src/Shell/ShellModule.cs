using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Shell.Application;

namespace Shell
{
    public class ShellModule : IModule
    {
        public void OnInitialized(IContainerProvider cp)
        {
            cp.Resolve<ModuleController>().Run();
        }

        public void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ModuleController>();


        }
    }

    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            Shell.Application.Bootstraper.RegisterTypes(cr);
            Shell.Presentation.Bootstraper.RegisterTypes(cr);
        }
    }
}