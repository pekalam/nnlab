﻿using System;
using System.Linq;
using System.Net.NetworkInformation;
using Common.Domain;
using Data.Application.Controllers;
using Data.Application.Controllers.DataSource;
using Data.Application.Services;
using Data.Application.ViewModels.DataSourceSelection;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;

namespace Data.Application
{
    internal class ModuleController
    {
        private IRegionManager _rm;
        private AppState _appState;
        private IEventAggregator _ea;

        //Instantiate singleton controllers
        private readonly IFileController _fileController;
        private readonly IDataSetDivisionController _dataSetDivisionController;
        private readonly IVariablesSelectionService _variablesSelectionController;
        private readonly IFileDataSourceController _fileDataSourceController;

        

        public ModuleController(IRegionManager rm, AppState appState, IFileController fileController,
            IDataSetDivisionController dataSetDivisionController,
            IVariablesSelectionService variablesSelectionController,
            IFileDataSourceController fileDataSourceController, IEventAggregator ea)
        {
            _rm = rm;
            _appState = appState;
            _fileController = fileController;
            _dataSetDivisionController = dataSetDivisionController;
            _variablesSelectionController = variablesSelectionController;
            _fileDataSourceController = fileDataSourceController;
            _ea = ea;

            _ea.GetEvent<SetupNewNavigationForSession>().Subscribe(OnSetupNewNavForSession);
            _ea.GetEvent<ReloadContentForSession>().Subscribe(OnReloadContentForSession);
        }

        private void OnReloadContentForSession((int moduleId, Session prev, Session next) arg)
        {
            if (arg.moduleId == ModuleIds.Data)
            {
                var view = GetViewToNavigateFromSession(arg);
                _rm.NavigateContentRegion(view);
            }
        }

        private void OnSetupNewNavForSession((int moduleId, Session prev, Session next) arg)
        {
            if(arg.moduleId == ModuleIds.Data)
            {
                var view = GetViewToNavigateFromSession(arg);
                _ea.OnFirstNavigation(ModuleIds.Data, () => _rm.NavigateContentRegion(view));
            }
        }

        private string GetViewToNavigateFromSession((int moduleId, Session prev, Session next) arg)
        {
            if (arg.next.TrainingData == null)
            {
                return "SelectDataSourceView";
            }
            else if (arg.next.TrainingData.Source == TrainingDataSource.Csv)
            {
                return "FileDataSourceView";

            }
            else if (arg.next.TrainingData.Source == TrainingDataSource.Memory)
            {
                return "CustomDataSetView";
            }

            throw new Exception("Invalid arg");
        }


        public void Run()
        {
            //Initialize singleton controllers
            _fileController.Initialize();


            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<CheckNavMenuItem>().Publish(ModuleIds.Data);
            _rm.NavigateContentRegion("SelectDataSourceView");
        }
    }
}