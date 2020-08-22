using Prism.Ioc;
using Prism.Regions;
using Shell.Application.PrismDecorators;
using Shell.Application.Services;
using Shell.Interface;

namespace Shell.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .Register<IRegionNavigationContentLoader, RegionNavigationContentLoaderDecorator>()
                .RegisterSingleton<IRegionManager, RegionManagerNavigationDecorator>();

            IStatusBarService.Register(cr);
        }
    }
}
