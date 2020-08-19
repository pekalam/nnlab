using System.ComponentModel;
using Infrastructure.Domain;
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
                .Register<IActionMenuNavigationService, ActionMenuNavigationService>()
                .RegisterSingleton<AppState>();
        }
    }
}