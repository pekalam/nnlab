using Prism.Ioc;
using Training.Application.Controllers;

namespace Training.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ModuleState>();

            cr.RegisterSingleton<ITrainingController, TrainingController>()
                .RegisterSingleton<ITrainingInfoController, TrainingInfoController>();

            IPanelSelectController.Register(cr);
            ITrainingController.Register(cr);
            IErrorPlotController.Register(cr);
            IOutputPlotController.Register(cr);
            IMatrixTrainingPreviewController.Register(cr);
            IReportsController.Register(cr);
            IReportErrorController.Register(cr);
            ITrainingParametersController.Register(cr);
            ITrainingNetworkPreviewController.Register(cr);
        }
    }
}