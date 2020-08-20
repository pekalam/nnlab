using System.ComponentModel;
using Infrastructure.Domain;
using Infrastructure.PrismDecorators;
using Prism.Ioc;
using Prism.Regions;

namespace Infrastructure
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .Register<IRegionNavigationContentLoader, RegionNavigationContentLoaderDecorator>()
                .RegisterSingleton<IRegionManager, RegionManagerNavigationDecorator>()
                .Register<IActionMenuNavigationService, ActionMenuNavigationService>()
                .RegisterSingleton<AppState>();
        }
    }
}