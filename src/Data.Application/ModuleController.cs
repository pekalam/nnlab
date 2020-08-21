using Common.Domain;
using Data.Application.Controllers;
using Data.Application.Controllers.DataSource;
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
        private readonly FileController _fileController;
        private readonly DataSetDivisionController _dataSetDivisionController;
        private readonly VariablesSelectionController _variablesSelectionController;
        private readonly NormalizationController _normalizationController;
        private readonly FileDataSourceController _fileDataSourceController;

        public ModuleController(IRegionManager rm, AppState appState, FileController fileController, DataSetDivisionController dataSetDivisionController, VariablesSelectionController variablesSelectionController, NormalizationController normalizationController, FileDataSourceController fileDataSourceController, IEventAggregator ea)
        {
            _rm = rm;
            _appState = appState;
            _fileController = fileController;
            _dataSetDivisionController = dataSetDivisionController;
            _variablesSelectionController = variablesSelectionController;
            _normalizationController = normalizationController;
            _fileDataSourceController = fileDataSourceController;
            _ea = ea;
        }


        public void Run()
        {
            //Initialize singleton controllers
            _fileController.Initialize();


            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<CheckNavMenuItem>().Publish(ModuleIds.Data);
            _rm.NavigateContentRegion(nameof(SelectDataSourceViewModel), "Data source");
        }
    }
}
