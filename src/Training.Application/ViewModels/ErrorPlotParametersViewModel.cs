using System;
using System.Collections.Generic;
using System.Text;
using Prism.Regions;
using Training.Application.Controllers;

namespace Training.Application.ViewModels
{
    public class ErrorPlotParametersViewModel : PlotEpochParametersViewModel
    {
        private IErrorPlotController _controller = null!;

        public IErrorPlotController Controller
        {
            get => _controller;
            set => SetProperty(ref _controller, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            Controller = (IErrorPlotController)navigationContext.Parameters["Controller"];
        }
    }
}
