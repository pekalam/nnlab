using System;
using System.IO;
using CommonServiceLocator;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Domain.Services;
using Data.Presentation.Services;
using Data.Presentation.Views;
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
                .RegisterSingleton<FileService>().RegisterSingleton<IFileService, FileService>()
                .RegisterSingleton<SingleFileService>().RegisterSingleton<ISingleFileService, SingleFileService>()
                .RegisterSingleton<MultiFileService>().RegisterSingleton<IMultiFileService, MultiFileService>()
                .RegisterSingleton<FileDialogService>().RegisterSingleton<IFileDialogService, FileDialogService>()
                .RegisterSingleton<CustomDataSetService>().RegisterSingleton<ICustomDataSetService, CustomDataSetService>()
                .RegisterSingleton<DataSetDivisionService>().RegisterSingleton<IDataSetDivisionService, DataSetDivisionService>();


            containerRegistry.RegisterSingleton<ModuleController>();

                
            containerRegistry.RegisterForNavigation<SelectDataSourceView>();
            containerRegistry.RegisterForNavigation<SingleFileSourceView>();
            containerRegistry.RegisterForNavigation<MultiFileSourceView>();
            containerRegistry.RegisterForNavigation<CustomDataSetView>();
            containerRegistry.RegisterForNavigation<DataSetDivisionView>();
        }
    }
}