using Common.Framework;
using Data.Application.Controllers;


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
