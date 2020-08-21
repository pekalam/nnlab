using Common.Framework;
using System.Windows.Input;

namespace Data.Application.Services
{
    public interface IVariablesSelectionService : IService
    {
        ICommand IgnoreAll { get; set; }
    }
}