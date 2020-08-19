using System;
using System.IO;
using CommonServiceLocator;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.VariablesSelection;
using Data.Domain.Services;
using Data.Presentation.Services;
using Data.Presentation.Views;
using Data.Presentation.Views.CustomDataSet;
using Data.Presentation.Views.DataSetDivision;
using Data.Presentation.Views.DataSource.FileDataSource;
using Data.Presentation.Views.DataSource.Normalization;
using Data.Presentation.Views.DataSource.Preview;
using Data.Presentation.Views.DataSource.VariablesSelection;
using Infrastructure;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Unity;

namespace Data
{
    public class DataModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<ModuleController>().Run();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry
                .Register<ISupervisedDataSetService, SupervisedDataSetService>()
                .Register<ICsvValidationService, CsvValidationService>()
                .Register<INormalizationDomainService,NormalizationDomainService>()
                .RegisterSingleton<IFileDialogService, FileDialogService>()
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

                .RegisterSingleton<ITransientControllerBase<VariablesSelectionService>, VariablesSelectionController>()
                .Register<IVariablesSelectionService, VariablesSelectionService>();

            containerRegistry.RegisterSingleton<ModuleController>();

                
            containerRegistry.RegisterForNavigation<SelectDataSourceView>();
            containerRegistry.RegisterForNavigation<SingleFileSourceView>();
            containerRegistry.RegisterForNavigation<MultiFileSourceView>();
            containerRegistry.RegisterForNavigation<CustomDataSetView>();
            containerRegistry.RegisterForNavigation<DataSetDivisionView>();
            containerRegistry.RegisterForNavigation<DataSourcePreviewView>();
            containerRegistry.RegisterForNavigation<FileDataSourceView>();
            containerRegistry.RegisterForNavigation<NormalizationView>();
            containerRegistry.RegisterForNavigation<VariablesSelectionView>();
        }
    }
}