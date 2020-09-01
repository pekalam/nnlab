using System.Diagnostics;
using Common.Framework;
using Data.Application.Services;
using Data.Domain;
using Unity;

namespace Data.Application.ViewModels.DataSource.VariablesSelection
{
    public class VariablesSelectionViewModel : ViewModelBase<VariablesSelectionViewModel>
    {
        private VariableTableModel[] _variables = null!;
        private string? _error;

        public VariablesSelectionViewModel()
        {
            
        }

        [InjectionConstructor]
        public VariablesSelectionViewModel(IVariablesSelectionService service)
        {
            Service = service;
        }


        public IVariablesSelectionService Service { get; }

        public VariableTableModel[] Variables
        {
            get => _variables;
            set
            {
                SetProperty(ref _variables, value);
                foreach (var model in value)
                {
                    model.OnError = error => Error = error;
                }
            }
        }

        public string? Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }
    }
}