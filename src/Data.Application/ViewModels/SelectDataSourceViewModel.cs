using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Services;

namespace Data.Application.ViewModels
{
    public class SelectDataSourceViewModel : ViewModelBase<SelectDataSourceViewModel>
    {
        public SelectDataSourceViewModel(IFileController fileService)
        {
            FileService = fileService;
        }

        public IFileController FileService { get; set; }
    }
}
