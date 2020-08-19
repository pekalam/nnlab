using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using ControlzEx.Standard;
using Data.Application.ViewModels;
using Infrastructure;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Mvvm;

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
