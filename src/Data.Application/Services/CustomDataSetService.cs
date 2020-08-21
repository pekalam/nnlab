using Common.Framework;
using OxyPlot;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;

namespace Data.Application.Services
{
    public interface ICustomDataSetService : INotifyPropertyChanged, IService
    {
        DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        DelegateCommand OpenDivisionViewCommand { get; set; }
        DelegateCommand SelectVariablesCommand { get; set; }
    }

    public class CustomDataSetService : BindableBase, ICustomDataSetService
    {
        public CustomDataSetService(ITransientControllerBase<CustomDataSetService> ctrl)
        {
            ctrl.Initialize(this);
        }

        public DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        public DelegateCommand OpenDivisionViewCommand { get; set; }
        public DelegateCommand SelectVariablesCommand { get; set; }
    }
}
