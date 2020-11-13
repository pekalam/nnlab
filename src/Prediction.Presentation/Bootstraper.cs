using Approximation.Application;
using Approximation.Presentation.Views;
using Prism.Ioc;

namespace Approximation.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ModuleState>();

            cr.RegisterForNavigation<ApproximationView>();
        }
    }
}