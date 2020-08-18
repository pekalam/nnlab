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
    public interface ICustomDataSetService : INotifyPropertyChanged
    {
        DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        DelegateCommand OpenDivisionViewCommand { get; set; }
    }

    public class CustomDataSetService : BindableBase, ICustomDataSetService
    {
        public DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        public DelegateCommand OpenDivisionViewCommand { get; set; }
    }
}
