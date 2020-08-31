#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
using Common.Framework;
using Data.Application.ViewModels.DataSourceSelection;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;
using Common.Domain;
using Data.Application.ViewModels;

namespace Data.Application.Services
{
    public interface IMultiFileService : INotifyPropertyChanged, IService
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
    }

    public class MultiFileService : BindableBase, IMultiFileService
    {
        public MultiFileService(ITransientController<MultiFileService> controller)
        {
            controller.Initialize(this);
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


        public FileValidationResult TrainingValidationResult => MultiFileSourceViewModel.Instance!.MultiFileValidationResult[0];
        public FileValidationResult ValidationValidationResult => MultiFileSourceViewModel.Instance!.MultiFileValidationResult[1];
        public FileValidationResult TestValidationResult => MultiFileSourceViewModel.Instance!.MultiFileValidationResult[2];


        public void SetTrainingValidationResult(bool isValid, string? error = null, bool hasContentError = false,
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

        public void SetValidationValidationResult(bool isValid, string? error = null, bool hasContentError = false,
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


        public void SetTestValidationResult(bool isValid, string? error = null, bool hasContentError = false, int r = 0,
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
            MultiFileSourceViewModel.Instance!.Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames)
                .Select((s, i) => new VariablesTableModel()
                {
                    Column = i + 1,
                    Name = s
                }).ToArray();
        }
    }
}