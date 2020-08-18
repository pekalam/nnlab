using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Data.Application.Controllers.DataSource;
using Data.Presentation.Views;
using Infrastructure;
using Infrastructure.Domain;
using Prism.Regions;

namespace Data.Application.Controllers
{
    internal class ModuleController
    {
        private IRegionManager _rm;
        private AppState _appState;

        //Instantiate singleton controllers
        private readonly FileController _fileController;
        private readonly DataSetDivisionController _dataSetDivisionController;
        private readonly VariablesSelectionController _variablesSelectionController;
        private readonly NormalizationController _normalizationController;
        private readonly FileDataSourceController _fileDataSourceController;

        public ModuleController(IRegionManager rm, AppState appState, FileController fileController, DataSetDivisionController dataSetDivisionController, VariablesSelectionController variablesSelectionController, NormalizationController normalizationController, FileDataSourceController fileDataSourceController)
        {
            _rm = rm;
            _appState = appState;
            _fileController = fileController;
            _dataSetDivisionController = dataSetDivisionController;
            _variablesSelectionController = variablesSelectionController;
            _normalizationController = normalizationController;
            _fileDataSourceController = fileDataSourceController;
        }


        public void Run()
        {
            //Initialize singleton controllers
            _fileController.Initialize();


            _rm.Regions[AppRegions.ContentRegion].RequestNavigate(nameof(SelectDataSourceView));
        }
    }
}
