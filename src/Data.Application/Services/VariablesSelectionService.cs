using System.Windows.Input;
using Common.Framework;

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