using Common.Framework;
using Data.Application.Interfaces;
using Data.Application.Services;
using Data.Application.ViewModels.CustomDataSet;
using Data.Application.ViewModels.DataSourceSelection;
using Prism.Commands;
using Prism.Regions;
using Shell.Interface;

namespace Data.Application.Controllers
{
    internal class FileController : ISingletonController
    {
        private readonly IRegionManager _rm;
        private readonly FileService _fileService;
        private readonly IFileDialogService _fileDialogService;

        public FileController(IRegionManager rm, FileService fileService, IFileDialogService fileDialogService)
        {
            _rm = rm;
            _fileService = fileService;
            _fileDialogService = fileDialogService;

            _fileService.CreateDataSetCommand = new DelegateCommand(CreateDataSet);
            _fileService.SelectFileCommand = new DelegateCommand(SelectFile);
            _fileService.LoadFilesCommand = new DelegateCommand(SelectFiles);
        }

        public void Initialize() { }

        private void SelectFiles()
        {
            
            _rm.NavigateContentRegion(nameof(MultiFileSourceViewModel), "Files");
        }


        private void SelectFile()
        {
            var dialog = _fileDialogService.OpenCsv();
            if (dialog.result == true)
            {
                _rm.NavigateContentRegion(nameof(SingleFileSourceViewModel), "File");
                SingleFileSourceViewModel.Instance.SelectedFilePath = dialog.filePath;
            }
        }

        private void CreateDataSet()
        {
            _rm.NavigateContentRegion(nameof(CustomDataSetViewModel), "Custom data set");
        }
    }
}
