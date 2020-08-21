using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Controllers.DataSource;
using Data.Application.Services;
using Prism.Ioc;

namespace Data.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr
                .RegisterSingleton<FileService>().RegisterSingleton<IFileService, FileService>()
                .RegisterSingleton<DataSetDivisionService>().RegisterSingleton<IDataSetDivisionService, DataSetDivisionService>()
                .RegisterSingleton<NormalizationService>().RegisterSingleton<INormalizationService, NormalizationService>()
                .RegisterSingleton<FileDataSourceService>().RegisterSingleton<IFileDataSourceService, FileDataSourceService>()


                .Register<ITransientControllerBase<SingleFileService>, SingleFileSourceController>()
                .Register<ISingleFileService, SingleFileService>()

                .Register<ITransientControllerBase<MultiFileService>, MultiFileSourceController>()
                .Register<IMultiFileService, MultiFileService>()


                .RegisterSingleton<ITransientControllerBase<CustomDataSetService>, CustomDataSetController>()
                .Register<ICustomDataSetService, CustomDataSetService>()

                .RegisterSingleton<ITransientControllerBase<StatisticsService>, StatisticsController>()
                .Register<IStatisticsService, StatisticsService>()

                .RegisterSingleton<ITransientControllerBase<VariablesSelectionService>, VariablesSelectionController>()
                .Register<IVariablesSelectionService, VariablesSelectionService>();
        }
    }
}
