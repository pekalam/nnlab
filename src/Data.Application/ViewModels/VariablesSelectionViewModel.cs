using Common.Framework;
using Data.Application.Controllers;
using Data.Domain;
using Unity;

namespace Data.Application.ViewModels
{
    public class VariablesSelectionViewModel : ViewModelBase<VariablesSelectionViewModel>
    {
        private VariableTableModel[] _variables = null!;
        private string? _error;

#pragma warning disable 8618
        public VariablesSelectionViewModel()
#pragma warning restore 8618
        {
            
        }

        [InjectionConstructor]
        public VariablesSelectionViewModel(IVariablesSelectionController controller)
        {
            Controller = controller;
            controller.Initialize(this);
        }


        public IVariablesSelectionController Controller { get; }

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