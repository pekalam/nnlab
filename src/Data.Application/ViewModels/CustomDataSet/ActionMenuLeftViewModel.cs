using Common.Framework;
using Data.Application.Services;
using Infrastructure;

namespace Data.Application.ViewModels.CustomDataSet
{
    public class ActionMenuLeftViewModel : ViewModelBase<ActionMenuLeftViewModel>
    {

        public ActionMenuLeftViewModel(ICustomDataSetService dataSetService)
        {
            DataSetService = dataSetService;
        }

        public ICustomDataSetService DataSetService { get; set; }
    }
}
