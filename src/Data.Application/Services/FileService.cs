using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
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
            cr.RegisterSingleton<IFileService, FileService>();
        }
    }

    public class FileService : IFileService
    {
        public DelegateCommand CreateDataSetCommand { get; set; }
        public DelegateCommand SelectFileCommand { get; set; }
        public DelegateCommand LoadFilesCommand { get; set; }
    }
}
