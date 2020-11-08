using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Data.Application.Controllers
{
    public interface ISingleFileController : ITransientController
    {
        DelegateCommand ReturnCommand { get; set; }
        DelegateCommand ContinueCommand { get; set; }
        DelegateCommand<string> ValidateCommand { get; set; }
        DelegateCommand<string> LoadCommand { get; set; }
    }

    internal class SingleFileSourceController : ControllerBase<SingleFileSourceViewModel>,ISingleFileController
    {
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

            ContinueCommand = new DelegateCommand(Continue, () => _loadedTrainingData != null);
            ValidateCommand = new DelegateCommand<string>(ValidateSingleFile);
            LoadCommand = new DelegateCommand<string>(LoadSingleFile, _ => _canLoad);
            ReturnCommand = new DelegateCommand(() =>
            {
                _loadedTrainingData = null;
                _rm.NavigateContentRegion("SelectDataSourceView");
            }, () => _canReturn);

            _ea.GetEvent<EnableModalNavigation>().Publish(ReturnCommand);
        }

        private void SetCanReturn(bool can)
        {
            _canReturn = can; 
            ReturnCommand.RaiseCanExecuteChanged();

        }

        private async void LoadSingleFile(string path)
        {
            SetCanReturn(false);
            Vm!.SetLoading();
            await Task.Run(() => _loadedTrainingData = _dataService.LoadDefaultTrainingData(path));
            Debug.Assert(_loadedTrainingData != null);
            Vm!.SetLoaded(_loadedTrainingData);
            SetCanReturn(true);
            ContinueCommand.RaiseCanExecuteChanged();
        }

        private async void ValidateSingleFile(string path)
        {
            SetCanReturn(false);
            Vm!.SetValidating();

            bool result = false;
            string? error = null;
            int r=0,c = 0;
            await Task.Run(() => (result, error, r, c) = _csvValidationService.Validate(path));


            SetCanReturn(true);

            if (result)
            {
                _canLoad = true;
                LoadCommand.RaiseCanExecuteChanged();
            }

            Vm!.SetValidated(result, r, c, error);
            
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

        public DelegateCommand ReturnCommand { get; set; }
        public DelegateCommand ContinueCommand { get; set; }
        public DelegateCommand<string> ValidateCommand { get; set; }
        public DelegateCommand<string> LoadCommand { get; set; }
    }
}