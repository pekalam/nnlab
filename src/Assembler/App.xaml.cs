﻿using Data;
using MathNet.Numerics;
using NeuralNetwork;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Shell;
using Shell.Presentation.Views;
using System;
using System.Reflection;
using Approximation;
using Training;
using Window = System.Windows.Window;

namespace Assemlber
{
    public partial class App
    {
        public App()
        {
            Control.TryUseNativeMKL();
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindowView>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<ShellModule>();
            moduleCatalog.AddModule<DataModule>();
            moduleCatalog.AddModule<NeuralNetworkModule>();
            moduleCatalog.AddModule<TrainingModule>();
            moduleCatalog.AddModule<ApproximationModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Common.Logging.Bootstraper.Configure();
            Common.Domain.Bootstraper.RegisterTypes(containerRegistry);
            Common.Framework.Bootstraper.RegisterTypes(containerRegistry);
            Common.Presentation.Bootstraper.RegisterTypes(containerRegistry);

            Shell.Bootstraper.RegisterTypes(containerRegistry);
        }


        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                string? viewName = null;
                if (viewType.FullName!.Contains(".Presentation"))
                {
                    viewName = viewType.FullName.Replace(".Presentation.Views.", ".Application.ViewModels.");
                }
                else
                {
                    viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
                }
                var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName!.Replace(".Presentation", ".Application");
                var viewModelName = viewName + "Model, " + viewAssemblyName;
                return Type.GetType(viewModelName);
            });
        }
    }
}
