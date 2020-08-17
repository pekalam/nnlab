using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Text;
using System.Windows.Input;
using Infrastructure.Domain;
using Prism.Commands;
using Prism.Mvvm;

namespace Data.Application.Services
{
    public interface IFileService : INotifyPropertyChanged
    {
        DelegateCommand CreateDataSetCommand { get; set; }
        DelegateCommand SelectFileCommand { get; set; }
        DelegateCommand LoadFilesCommand { get; set; }
    }

    public class FileService : BindableBase, IFileService
    {
        public DelegateCommand CreateDataSetCommand { get; set; }
        public DelegateCommand SelectFileCommand { get; set; }
        public DelegateCommand LoadFilesCommand { get; set; }
    }
}
