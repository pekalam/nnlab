using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.FileDataSource;
using Data.Application.ViewModels.DataSourceSelection;
using Data.Domain.Services;
using Infrastructure.Domain;
using Prism.Commands;
using Prism.Regions;
using Shell.Interface;
using System.Threading.Tasks;

namespace Data.Application.Controllers
{
    internal class SingleFileSourceController : ITransientController<SingleFileService>
    {
        private SingleFileService _singleFileService;
        private bool _canLoad;
        private bool _canReturn = true;
        private TrainingData _loadedTrainingData;
        private readonly ISupervisedDataSetService _dataSetService;
        private readonly ICsvValidationService _csvValidationService;
        private readonly IRegionManager _rm;
        private readonly AppState _appState;

        public SingleFileSourceController(ISupervisedDataSetService dataSetService, ICsvValidationService csvValidationService, IRegionManager rm, AppState appState)
        {
            _dataSetService = dataSetService;
            _csvValidationService = csvValidationService;
            _rm = rm;
            _appState = appState;
        }

        public void Initialize(SingleFileService service)
        {
            _singleFileService = service;

            _singleFileService.ContinueCommand = new DelegateCommand(Continue, () => _loadedTrainingData != null);
            _singleFileService.ValidateCommand = new DelegateCommand<string>(ValidateSingleFile);
            _singleFileService.LoadCommand = new DelegateCommand<string>(LoadSingleFile, _ => _canLoad);
            _singleFileService.ReturnCommand = new DelegateCommand(() =>
            {
                _loadedTrainingData = null;
                _rm.NavigateContentRegion(nameof(SelectDataSourceViewModel), "Data source");
            }, () => _canReturn);
        }


        private void SetCanReturn(bool can)
        {
            _canReturn = can; 
            _singleFileService.ReturnCommand.RaiseCanExecuteChanged();

        }

        private async void LoadSingleFile(string path)
        {
            SetCanReturn(false);
            _singleFileService.SetLoading();
            await Task.Run(() => _loadedTrainingData = _dataSetService.LoadDefaultSet(path));
            _singleFileService.SetLoaded(_loadedTrainingData);
            SetCanReturn(true);
            _singleFileService.ContinueCommand.RaiseCanExecuteChanged();
        }

        private async void ValidateSingleFile(string path)
        {
            SetCanReturn(false);
            _singleFileService.SetValidating();

            bool result = false;
            string error = null;
            int r=0,c = 0;
            await Task.Run(() => (result, error, r, c) = _csvValidationService.Validate(path));


            SetCanReturn(true);

            if (result)
            {
                _canLoad = true;
                _singleFileService.LoadCommand.RaiseCanExecuteChanged();
            }

            _singleFileService.SetValidated(result, r, c, error);
            
        }

        private void Continue()
        {
            var session = _appState.SessionManager.Create();

            session.TrainingData = _loadedTrainingData;
            session.SingleDataFile = SingleFileSourceViewModel.Instance.SelectedFilePath;
            _loadedTrainingData = null;

            _rm.NavigateContentRegion(nameof(FileDataSourceViewModel), "Data");
        }
    }
}