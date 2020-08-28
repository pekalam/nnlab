using Data.Domain.Services;
using Prism.Ioc;

namespace Data.Domain
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.Register<ITrainingDataService, TrainingDataService>()
                .Register<ICsvValidationService, CsvValidationService>()
                .Register<INormalizationDomainService, NormalizationDomainService>()
                .RegisterSingleton<ModuleState>();
        }
    }
}