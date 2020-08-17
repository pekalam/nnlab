using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Infrastructure.Domain;
using Prism.Commands;
using Prism.Mvvm;

namespace Data.Application.Services
{
    //TODO move state to view
    public interface IMultiFileService : INotifyPropertyChanged
    {
        DelegateCommand SelectTrainingFileCommand { get; set; }
        DelegateCommand SelectValidationFileCommand { get; set; }
        DelegateCommand SelectTestFileCommand { get; set; }

        DelegateCommand ReturnCommand { get; set; }
        DelegateCommand ContinueCommand { get; set; }

        DelegateCommand<string> ValidateTrainingFile { get; set; }
        DelegateCommand<string> ValidateTestFile { get; set; }
        DelegateCommand<string> ValidateValidationFile { get; set; }

        DelegateCommand<(string trainingFile, string validationFile, string testFile)?> LoadFiles { get; set; }
        
        ObservableCollection<FileValidationResult> MultiFileValidationResult { get; set; }

        VariablesTableModel[] Variables { get; set; }
    }

    public class MultiFileService : BindableBase, IMultiFileService
    {
        private FileValidationResult _trainingValidationResult;
        private FileValidationResult _validationValidationResult;
        private FileValidationResult _testValidationResult;
        private VariablesTableModel[] _variables;

        public MultiFileService()
        {
            MultiFileValidationResult = new ObservableCollection<FileValidationResult>(Enumerable.Repeat(new FileValidationResult(), 3));
        }

        public DelegateCommand SelectTrainingFileCommand { get; set; }
        public DelegateCommand SelectValidationFileCommand { get; set; }
        public DelegateCommand SelectTestFileCommand { get; set; }
        public DelegateCommand ReturnCommand { get; set; }
        public DelegateCommand ContinueCommand { get; set; }
        public DelegateCommand<string> ValidateTrainingFile { get; set; }
        public DelegateCommand<string> ValidateTestFile { get; set; }
        public DelegateCommand<string> ValidateValidationFile { get; set; }
        public DelegateCommand<(string trainingFile, string validationFile, string testFile)?> LoadFiles { get; set; }
        public ObservableCollection<FileValidationResult> MultiFileValidationResult { get; set; }

        public VariablesTableModel[] Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value);
        }


        public FileValidationResult TrainingValidationResult => MultiFileValidationResult[0];
        public FileValidationResult ValidationValidationResult => MultiFileValidationResult[1];
        public FileValidationResult TestValidationResult => MultiFileValidationResult[2];

        public void Reset()
        {
            MultiFileValidationResult = new ObservableCollection<FileValidationResult>(Enumerable.Repeat(new FileValidationResult(), 3));
        }

        public void ResetResult(int num)
        {
            MultiFileValidationResult[num] = new FileValidationResult();
            Variables = null;
        }

        public void SetTrainingValidationResult(bool isValid, string error = null, bool hasContentError = false,
            int r = 0, int c = 0)
        {
            TrainingValidationResult.Rows = r;
            TrainingValidationResult.Cols = c;
            TrainingValidationResult.IsValidatingFile = false;
            TrainingValidationResult.IsFileValid = isValid;
            TrainingValidationResult.HasContentError = hasContentError;
            TrainingValidationResult.FileValidationError = error;
        }

        public void SetTrainingValidating(bool isValidating) =>
            TrainingValidationResult.IsValidatingFile = isValidating;

        public void SetValidationValidationResult(bool isValid, string error = null, bool hasContentError = false,
            int r = 0, int c = 0)
        {
            ValidationValidationResult.Rows = r;
            ValidationValidationResult.Cols = c;
            ValidationValidationResult.IsValidatingFile = false;
            ValidationValidationResult.IsFileValid = isValid;
            ValidationValidationResult.HasContentError = hasContentError;
            ValidationValidationResult.FileValidationError = error;
        }

        public void SetValidationValidating(bool isValidating) =>
            ValidationValidationResult.IsValidatingFile = isValidating;


        public void SetTestValidationResult(bool isValid, string error = null, bool hasContentError = false, int r = 0,
            int c = 0)
        {
            TestValidationResult.Rows = r;
            TestValidationResult.Cols = c;
            TestValidationResult.IsValidatingFile = false;
            TestValidationResult.IsFileValid = isValid;
            TestValidationResult.HasContentError = hasContentError;
            TestValidationResult.FileValidationError = error;
        }

        public void SetTestValidating(bool isValidating) =>
            TestValidationResult.IsValidatingFile = isValidating;


        public void SetIsLoading() => TrainingValidationResult.IsLoadingFile =
            ValidationValidationResult.IsLoadingFile = TestValidationResult.IsLoadingFile = true;

        public void SetLoaded(TrainingData trainingData)
        {
            TrainingValidationResult.IsLoadingFile =
                ValidationValidationResult.IsLoadingFile = TestValidationResult.IsLoadingFile = false;
            TrainingValidationResult.IsLoaded =
                ValidationValidationResult.IsLoaded = TestValidationResult.IsLoaded = true;
            Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames)
                .Select((s, i) => new VariablesTableModel()
                {
                    Column = i + 1,
                    Name = s
                }).ToArray();
        }
    }
}