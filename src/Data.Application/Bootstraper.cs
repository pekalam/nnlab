using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Controllers.DataSource;
using Data.Application.Services;
using Prism.Ioc;
using Prism.Unity;

namespace Data.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .RegisterSingleton<FileService>().RegisterSingleton<IFileService, FileService>()
                .RegisterSingleton<DataSetDivisionService>()
                .RegisterSingleton<IDataSetDivisionService, DataSetDivisionService>()
                .RegisterSingleton<FileDataSourceService>()
                .RegisterSingleton<IFileDataSourceService, FileDataSourceService>();

                INormalizationService.Register(cr);
                IStatisticsService.Register(cr);
                ICustomDataSetService.Register(cr);
                IVariablesSelectionService.Register(cr);

                cr.Register<ITransientController<SingleFileService>, SingleFileSourceController>()
                    .Register<ISingleFileService, SingleFileService>()

                    .Register<ITransientController<MultiFileService>, MultiFileSourceController>()
                    .Register<IMultiFileService, MultiFileService>();
        }
    }
}
