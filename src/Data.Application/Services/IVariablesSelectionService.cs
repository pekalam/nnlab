using System.Windows.Input;
using Common.Framework;
using Infrastructure;

namespace Data.Application.Services
{
    public interface IVariablesSelectionService : IService
    {
        ICommand IgnoreAll { get; set; }
    }
}