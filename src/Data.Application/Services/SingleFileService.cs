using Common.Framework;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Common.Domain;
using Data.Application.ViewModels;

namespace Data.Application.Services
{
    public class VariablesTableModel
    {
        public int Column { get; set; }
        public string Name { get; set; } = null!;
    }

    public interface ISingleFileService : INotifyPropertyChanged, IService
    {
        DelegateCommand ReturnCommand { get; set; }
        DelegateCommand ContinueCommand { get; set; }
        DelegateCommand<string> ValidateCommand { get; set; }
        DelegateCommand<string> LoadCommand { get; set; }
    }

    internal class SingleFileService : BindableBase, ISingleFileService
    {
        private IViewModelAccessor _accessor;

        public SingleFileService(ITransientController<SingleFileService> controller, IViewModelAccessor accessor)
        {
            _accessor = accessor;
            controller.Initialize(this);
        }

        public DelegateCommand ReturnCommand { get; set; } = null!;
        public DelegateCommand ContinueCommand { get; set; } = null!;
        public DelegateCommand<string> ValidateCommand { get; set; } = null!;
        public DelegateCommand<string> LoadCommand { get; set; } = null!;

        public FileValidationResult? FileValidationResult => _accessor.Get<SingleFileSourceViewModel>()?.FileValidationResult;

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
            _accessor.Get<SingleFileSourceViewModel>()!.Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames)
                .Select((s, i) => new VariablesTableModel()
                {
                    Column = i+1, Name = s
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