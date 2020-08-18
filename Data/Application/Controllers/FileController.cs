﻿using System;
using System.Collections.Generic;
using System.Text;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Presentation.Services;
using Data.Presentation.Views;
using Data.Presentation.Views.CustomDataSet;
using Infrastructure;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Regions;

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
