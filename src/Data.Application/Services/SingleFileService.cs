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

    public interface ISingleFileService : IService, ITransientController
    {
        DelegateCommand ReturnCommand { get; set; }
        DelegateCommand ContinueCommand { get; set; }
        DelegateCommand<string> ValidateCommand { get; set; }
        DelegateCommand<string> LoadCommand { get; set; }
    }
}