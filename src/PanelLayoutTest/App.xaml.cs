using System;
using System.Reflection;
using Prism.Ioc;
using PanelLayoutTest.Views;
using System.Windows;
using Prism.Modularity;
using Prism.Mvvm;
using Training;

namespace PanelLayoutTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Common.Domain.Bootstraper.RegisterTypes(containerRegistry);
            Common.Framework.Bootstraper.RegisterTypes(containerRegistry);
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<TrainingModule>();
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
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName.Replace(".Presentation", ".Application");
                var viewModelName = viewName + "Model, " + viewAssemblyName;
                return Type.GetType(viewModelName);
            });
        }
    }
}
