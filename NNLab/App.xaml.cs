using System;
using System.Reflection;
using Prism.Ioc;
using NNLab.Views;
using System.Windows;
using Data;
using Infrastructure;
using NeuralNetwork;
using Prism.Modularity;
using Prism.Mvvm;
using Training;
using Prism.Regions;

namespace NNLab
{
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindowView>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<DataModule>();
            moduleCatalog.AddModule<NeuralNetworkModule>();
            moduleCatalog.AddModule<TrainingModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Infrastructure.Bootstraper.RegisterTypes(containerRegistry);
            containerRegistry.Register<IContentRegionHistoryService, ContentRegionHistoryService>();
        }


        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                string viewName = null;
                if (viewType.FullName.Contains(".Presentation"))
                {
                    viewName = viewType.FullName.Replace(".Presentation.Views.", ".Application.ViewModels.");
                }
                else
                {
                    viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
                }
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
                var viewModelName = viewName + "Model, " + viewAssemblyName;
                return Type.GetType(viewModelName);
            });
        }
    }
}
