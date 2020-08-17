using System;
using System.ComponentModel;
using System.Linq;
using Infrastructure.Domain;
using Prism.Commands;
using Prism.Mvvm;

namespace Data.Application.Services
{
    public class VariablesTableModel
    {
        public int Column { get; set; }
        public string Name { get; set; }
    }

    public interface ISingleFileService : INotifyPropertyChanged
    {
        FileValidationResult FileValidationResult { get; set; }
        VariablesTableModel[] Variables { get; set; }

        DelegateCommand ReturnCommand { get; set; }
        DelegateCommand ContinueCommand { get; set; }
        DelegateCommand<string> ValidateCommand { get; set; }
        DelegateCommand<string> LoadCommand { get; set; }
    }

    public class SingleFileService : BindableBase, ISingleFileService
    {
        private VariablesTableModel[] _variables;

        public DelegateCommand ReturnCommand { get; set; }
        public DelegateCommand ContinueCommand { get; set; }
        public DelegateCommand<string> ValidateCommand { get; set; }
        public DelegateCommand<string> LoadCommand { get; set; }

        public FileValidationResult FileValidationResult { get; set; } = new FileValidationResult();
        public VariablesTableModel[] Variables
        {
            get => _variables;
            set
            {
                _variables = value;
                RaisePropertyChanged();
            }
        }

        public void SetLoading()
        {
            FileValidationResult.IsValidatingFile = false;
            FileValidationResult.IsLoadingFile = true;
        }

        public void SetValidating()
        {
            FileValidationResult.IsLoadingFile = false;
            FileValidationResult.IsValidatingFile = true;
        }

        public void SetLoaded(TrainingData trainingData)
        {
            FileValidationResult.IsValidatingFile = FileValidationResult.IsLoadingFile = false;
            FileValidationResult.IsLoaded = true;
            Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames)
                .Select((s, i) => new VariablesTableModel()
                {
                    Column = i+1, Name = s
                }).ToArray();
        }

        public void SetValidated(bool result, int rows, int cols, string error)
        {
            FileValidationResult.IsValidatingFile = FileValidationResult.IsLoadingFile = false;
            FileValidationResult.FileValidationError = error;
            FileValidationResult.IsFileValid = result;
            FileValidationResult.Rows = rows;
            FileValidationResult.Cols = cols;
        }


        public void Reset()
        {
            FileValidationResult = new FileValidationResult();
            Variables = default;
        }
    }
}