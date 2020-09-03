using System.Diagnostics;
using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.FileDataSource;
using Data.Application.ViewModels.DataSourceSelection;
using Data.Domain.Services;
using Prism.Commands;
using Prism.Regions;
using Shell.Interface;
using System.Threading.Tasks;
using Common.Domain;
using Prism.Events;

namespace Data.Application.Controllers
{
    internal class SingleFileSourceController : ITransientController<SingleFileService>
    {
        private SingleFileService _singleFileService = null!;
        private bool _canLoad;
        private bool _canReturn = true;
        private TrainingData? _loadedTrainingData;
        private readonly ITrainingDataService _dataService;
        private readonly ICsvValidationService _csvValidationService;
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;
        private readonly AppState _appState;

        public SingleFileSourceController(ITrainingDataService dataService, ICsvValidationService csvValidationService, IRegionManager rm, AppState appState, IEventAggregator ea)
        {
            _dataService = dataService;
            _csvValidationService = csvValidationService;
            _rm = rm;
            _appState = appState;
            _ea = ea;
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
                _rm.NavigateContentRegion("SelectDataSourceView");
            }, () => _canReturn);

            _ea.GetEvent<EnableModalNavigation>().Publish(_singleFileService.ReturnCommand);
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
            await Task.Run(() => _loadedTrainingData = _dataService.LoadDefaultSet(path));
            Debug.Assert(_loadedTrainingData != null);
            _singleFileService.SetLoaded(_loadedTrainingData);
            SetCanReturn(true);
            _singleFileService.ContinueCommand.RaiseCanExecuteChanged();
        }

        private async void ValidateSingleFile(string path)
        {
            SetCanReturn(false);
            _singleFileService.SetValidating();

            bool result = false;
            string? error = null;
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
            if (_appState.Sessions.Count == 0)
            {
                _appState.CreateSession();
            }

            _appState.ActiveSession!.TrainingData = _loadedTrainingData;
            _appState.ActiveSession!.SingleDataFile = SingleFileSourceViewModel.Instance!.SelectedFilePath;
            _loadedTrainingData = null;

            _ea.GetEvent<DisableModalNavigation>().Publish();
            _rm.NavigateContentRegion("FileDataSourceView");
        }
    }
}