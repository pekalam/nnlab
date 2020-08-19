using System.Windows.Input;
using Data.Application.ViewModels.DataSource.VariablesSelection;
using Infrastructure;

namespace Data.Application.Services
{
    public class VariablesSelectionService : IVariablesSelectionService
    {
        public VariablesSelectionService(ITransientControllerBase<VariablesSelectionService> ctrl)
        {
            ctrl.Initialize(this);
        }

        public ICommand IgnoreAll { get; set; }
    }
}