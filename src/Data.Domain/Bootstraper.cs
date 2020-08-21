using Data.Domain.Services;
using Prism.Ioc;

namespace Data.Domain
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.Register<ISupervisedDataSetService, SupervisedDataSetService>()
                .Register<ICsvValidationService, CsvValidationService>()
                .Register<INormalizationDomainService, NormalizationDomainService>();
        }
    }
}