using Prism.Ioc;
using Shell.Application;
using Shell.Application.Interfaces;
using Shell.Interface;

namespace Shell.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .Register<IContentRegionHistoryService, ContentRegionHistoryService>();
        }
    }
}