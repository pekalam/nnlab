using Common.Framework;
using Data.Application.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Common.Domain;
using Data.Application.Controllers;

namespace Data.Application.ViewModels
{
    public class VariablesTableModel
    {
        public int Column { get; set; }
        public string Name { get; set; } = null!;
    }

    public class SingleFileSourceViewModel : ViewModelBase<SingleFileSourceViewModel>
    {
        private string? _selectedFilePath;
        private string? _selectedFileName;
        private VariablesTableModel[]? _variables;

        public SingleFileSourceViewModel(ISingleFileController singleFileService)
        {
            SingleFileService = singleFileService;
            KeepAlive = false;
            FileValidationResult.PropertyChanged += FileValidationResultOnPropertyChanged;

            singleFileService.Initialize(this);
        }

        private void FileValidationResultOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FileValidationResult.IsFileValid):
                    if (FileValidationResult.IsFileValid == true)
                    {
                        SingleFileService.LoadCommand.Execute(_selectedFilePath!);
                    }
                    break;
            }
        }

        public ISingleFileController SingleFileService { get; }

        public FileValidationResult FileValidationResult { get; set; } = new FileValidationResult();
        public VariablesTableModel[]? Variables
        {
            get => _variables;
            set
            {
                _variables = value;
                RaisePropertyChanged();
            }
        }

        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                Debug.Assert(value != null);
                SetProperty(ref _selectedFilePath, value);
                SelectedFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                SingleFileService.ValidateCommand.Execute(value);
            }
        }

        public string? SelectedFileName
        {
            get => _selectedFileName;
            set => SetProperty(ref _selectedFileName, value);
        }


        public void SetLoading()
        {
            Debug.Assert(FileValidationResult != null, nameof(FileValidationResult) + " != null");
            FileValidationResult.IsValidatingFile = false;
            FileValidationResult.IsLoadingFile = true;
        }

        public void SetValidating()
        {
            Debug.Assert(FileValidationResult != null, nameof(FileValidationResult) + " != null");
            FileValidationResult.IsLoadingFile = false;
            FileValidationResult.IsValidatingFile = true;
        }

        public void SetLoaded(TrainingData trainingData)
        {
            Debug.Assert(FileValidationResult != null, nameof(FileValidationResult) + " != null");
            FileValidationResult.IsValidatingFile = FileValidationResult.IsLoadingFile = false;
            FileValidationResult.IsLoaded = true;
                Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames)
                .Select((s, i) => new VariablesTableModel()
                {
                    Column = i + 1,
                    Name = s
                }).ToArray();
        }

        public void SetValidated(bool result, int rows, int cols, string? error)
        {
            Debug.Assert(FileValidationResult != null, nameof(FileValidationResult) + " != null");
            FileValidationResult.IsValidatingFile = FileValidationResult.IsLoadingFile = false;
            FileValidationResult.FileValidationError = error;
            FileValidationResult.IsFileValid = result;
            FileValidationResult.Rows = rows;
            FileValidationResult.Cols = cols;
        }
    }
}
