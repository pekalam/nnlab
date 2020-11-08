using Prediction.Application.Controllers;
using Prism.Ioc;

namespace Prediction.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            IPredictController.Register(cr);
        }
    }
}