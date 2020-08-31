using Prediction.Application.Controllers;
using Prediction.Presentation.Views;
using Prism.Ioc;

namespace Prediction.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ModuleState>();

            cr.RegisterForNavigation<PredictView>();
        }
    }
}