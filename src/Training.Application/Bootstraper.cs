using Prism.Ioc;
using Training.Application.Controllers;
using Training.Application.Services;

namespace Training.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ITrainingController, TrainingController>()
                .RegisterSingleton<ITrainingInfoController, TrainingInfoController>();

            IPanelSelectService.Register(cr);
            ITrainingService.Register(cr);
        }
    }
}