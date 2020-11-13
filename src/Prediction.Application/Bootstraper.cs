using Approximation.Application.Controllers;
using Prism.Ioc;

namespace Approximation.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            IApproximationController.Register(cr);
        }
    }
}