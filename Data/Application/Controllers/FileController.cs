using System;
using System.Collections.Generic;
using System.Text;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Presentation.Services;
using Data.Presentation.Views;
using Infrastructure;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;

namespace Data.Application.Controllers
{
    internal class FileController
    {
        private readonly IRegionManager _rm;
        private readonly FileService _fileService;
        private readonly MultiFileService _multiFileService;
        private readonly IFileDialogService _fileDialogService;

        public FileController(IRegionManager rm, FileService fileService, IFileDialogService fileDialogService, MultiFileService multiFileService)
        {
            _rm = rm;
            _fileService = fileService;
            _fileDialogService = fileDialogService;
            _multiFileService = multiFileService;

            _fileService.CreateDataSetCommand = new DelegateCommand(CreateDataSet);
            _fileService.SelectFileCommand = new DelegateCommand(SelectFile);
            _fileService.LoadFilesCommand = new DelegateCommand(SelectFiles);

            _multiFileService.SelectTrainingFileCommand = new DelegateCommand(() => SelectMultiSetFile(0));
            _multiFileService.SelectValidationFileCommand = new DelegateCommand(() => SelectMultiSetFile(1));
            _multiFileService.SelectTestFileCommand = new DelegateCommand(() => SelectMultiSetFile(2));
        }

        public void Initialize() { }


        private void SelectMultiSetFile(int file)
        {
            var dialog = _fileDialogService.OpenCsv(null);
            if (dialog.result == true)
            {
                _multiFileService.ResetResult(file);
                if (file == 0)
                {
                    MultiFileSourceViewModel.Instance.TrainingSetFilePath = dialog.filePath;
                }
                else if (file == 1)
                {
                    MultiFileSourceViewModel.Instance.ValidationSetFilePath = dialog.filePath;
                }
                else if (file == 2)
                {
                    MultiFileSourceViewModel.Instance.TestSetFilePath = dialog.filePath;
                }
            }
        }

        private void SelectFiles()
        {
            _rm.Regions[AppRegions.ContentRegion].RequestNavigate(nameof(MultiFileSourceView));
        }


        private void SelectFile()
        {
            var dialog = _fileDialogService.OpenCsv(null);
            if (dialog.result == true)
            {
                _rm.Regions[AppRegions.ContentRegion].RequestNavigate(nameof(SingleFileSourceView));
                SingleFileSourceViewModel.Instance.SelectedFilePath = dialog.filePath;
            }
        }

        private void CreateDataSet()
        {
            _rm.Regions[AppRegions.ContentRegion].RequestNavigate(nameof(CustomDataSetView));
        }
    }
}
