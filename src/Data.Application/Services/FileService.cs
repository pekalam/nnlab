using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
using Data.Application.Controllers;
using Prism.Ioc;

namespace Data.Application.Services
{
    public interface IFileService
    {
        DelegateCommand CreateDataSetCommand { get; set; }
        DelegateCommand SelectFileCommand { get; set; }
        DelegateCommand LoadFilesCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IFileService, FileController>();
        }
    }
}
