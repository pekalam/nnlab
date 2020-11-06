using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Controllers.DataSource;
using Data.Application.Services;
using Data.Domain;
using Prism.Ioc;
using Prism.Unity;

namespace Data.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .RegisterSingleton<IFileService, FileController>()
                .RegisterSingleton<IDataSetDivisionService, DataSetDivisionController>()
                .RegisterSingleton<IFileDataSourceService, FileDataSourceController>();

                INormalizationService.Register(cr);
                IStatisticsService.Register(cr);
                ICustomDataSetService.Register(cr);
                IVariablesSelectionService.Register(cr);

                IFileController.Register(cr);
                IFileDataSourceController.Register(cr);
                IDataSetDivisionController.Register(cr);

                cr.Register<ISingleFileService, SingleFileSourceController>()

                    .Register<IMultiFileService, MultiFileSourceController>();
        }
    }
}
