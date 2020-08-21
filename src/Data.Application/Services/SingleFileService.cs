using Common.Framework;
using Data.Application.ViewModels.DataSourceSelection;
using Infrastructure.Domain;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;
using Data.Application.ViewModels;

namespace Data.Application.Services
{
    public class VariablesTableModel
    {
        public int Column { get; set; }
        public string Name { get; set; }
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
        public SingleFileService(ITransientController<SingleFileService> controller)
        {
            controller.Initialize(this);
        }

        public DelegateCommand ReturnCommand { get; set; }
        public DelegateCommand ContinueCommand { get; set; }
        public DelegateCommand<string> ValidateCommand { get; set; }
        public DelegateCommand<string> LoadCommand { get; set; }

        public FileValidationResult FileValidationResult => SingleFileSourceViewModel.Instance.FileValidationResult;

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
            SingleFileSourceViewModel.Instance.Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames)
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
    }
}