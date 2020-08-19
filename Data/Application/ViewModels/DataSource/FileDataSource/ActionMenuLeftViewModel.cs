using System.Windows.Input;
using Data.Application.Services;
using Infrastructure;

namespace Data.Application.ViewModels.DataSource.FileDataSource
{
    public class ActionMenuLeftViewModel : ViewModelBase<ActionMenuLeftViewModel>
    {
        public ActionMenuLeftViewModel(IFileDataSourceService service)
        {
            Service = service;
        }

        public IFileDataSourceService Service { get; set; }
    }
}
