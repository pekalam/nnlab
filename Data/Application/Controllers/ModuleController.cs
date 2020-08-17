using System;
using System.Collections.Generic;
using System.Text;
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

        public ModuleController(IRegionManager rm, FileController fileController, AppState appState, SingleFileSourceController singleFileSourceController, MultiFileSourceController multiFileSourceController)
        {
            _rm = rm;
            _fileController = fileController;
            _appState = appState;
            _singleFileSourceController = singleFileSourceController;
            _multiFileSourceController = multiFileSourceController;
        }



        public void Run()
        {
            _fileController.Initialize();


            _rm.Regions[AppRegions.ContentRegion].RequestNavigate(nameof(SelectDataSourceView));
        }
    }
}
