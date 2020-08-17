using System.Threading.Tasks;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Data.Presentation.Views;
using Infrastructure;
using Infrastructure.Domain;
using Prism.Commands;
using Prism.Regions;

namespace Data.Application.Controllers
{
    internal class SingleFileSourceController
    {
        private readonly SingleFileService _singleFileService;
        private bool _canLoad;
        private bool _canReturn = true;
        private TrainingData _loadedTrainingData;
        private readonly ISupervisedDataSetService _dataSetService;
        private readonly ICsvValidationService _csvValidationService;
        private readonly IRegionManager _rm;

        public SingleFileSourceController(SingleFileService singleFileService, ISupervisedDataSetService dataSetService, ICsvValidationService csvValidationService, IRegionManager rm)
        {
            _singleFileService = singleFileService;
            _dataSetService = dataSetService;
            _csvValidationService = csvValidationService;
            _rm = rm;


            _singleFileService.ContinueCommand = new DelegateCommand(SingleFileContinue, () => _loadedTrainingData != null);
            _singleFileService.ValidateCommand = new DelegateCommand<string>(ValidateSingleFile);
            _singleFileService.LoadCommand = new DelegateCommand<string>(LoadSingleFile, _ => _canLoad);
            _singleFileService.ReturnCommand = new DelegateCommand(() =>
            {
                _loadedTrainingData = null;
                _rm.Regions[AppRegions.ContentRegion].RequestNavigate(nameof(SelectDataSourceView));
            }, () => _canReturn);

            //reset state when new vm is created
            SingleFileSourceViewModel.Created += () =>
            {
                _loadedTrainingData = null;
                _canLoad = false;
                _canReturn = true;
                _singleFileService.Reset();
            };
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

        private void SingleFileContinue()
        {

        }
    }
}