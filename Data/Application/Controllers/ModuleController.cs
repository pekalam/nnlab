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
        private readonly FileController _fileController;
        private readonly SingleFileSourceController _singleFileSourceController;
        private readonly MultiFileSourceController _multiFileSourceController;
        private readonly CustomDataSetController _customDataSetController;
        private readonly VariablesSelectionController _variablesSelectionController;
        private readonly DataSetDivisionController _dataSetDivisionController;
        private readonly FileDataSourceController _fileDataSourceController;

        public ModuleController(IRegionManager rm, FileController fileController, AppState appState, SingleFileSourceController singleFileSourceController, MultiFileSourceController multiFileSourceController, CustomDataSetController customDataSetController, VariablesSelectionController variablesSelectionController, DataSetDivisionController dataSetDivisionController, FileDataSourceController fileDataSourceController)
        {
            _rm = rm;
            _fileController = fileController;
            _appState = appState;
            _singleFileSourceController = singleFileSourceController;
            _multiFileSourceController = multiFileSourceController;
            _customDataSetController = customDataSetController;
            _variablesSelectionController = variablesSelectionController;
            _dataSetDivisionController = dataSetDivisionController;
            _fileDataSourceController = fileDataSourceController;
        }



        public void Run()
        {
            _fileController.Initialize();


            _rm.Regions[AppRegions.ContentRegion].RequestNavigate(nameof(SelectDataSourceView));
        }
    }
}
