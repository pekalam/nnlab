using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Data.Application.Services;
using Data.Application.Views;
using Infrastructure;
using Prism.Mvvm;

namespace Data.Application.ViewModels
{
    public class SingleFileSourceViewModel : ViewModelBase<SingleFileSourceViewModel>
    {
        private string _selectedFilePath;
        private string _selectedFileName;

        public SingleFileSourceViewModel(ISingleFileService singleFileService)
        {
            SingleFileService = singleFileService;
            KeepAlive = false;
            SingleFileService.FileValidationResult.PropertyChanged += FileValidationResultOnPropertyChanged;
        }

        private void FileValidationResultOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FileValidationResult.IsFileValid):
                    if (SingleFileService.FileValidationResult.IsFileValid == true)
                    {
                        SingleFileService.LoadCommand.Execute(_selectedFilePath);
                    }
                    break;
            }
        }

        public ISingleFileService SingleFileService { get; }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                SetProperty(ref _selectedFilePath, value);
                SelectedFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                SingleFileService.ValidateCommand.Execute(value);
            }
        }

        public string SelectedFileName
        {
            get => _selectedFileName;
            set => SetProperty(ref _selectedFileName, value);
        }
    }
}
