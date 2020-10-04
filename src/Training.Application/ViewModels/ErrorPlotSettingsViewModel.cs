using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Common.Framework;
using Microsoft.VisualBasic;
using OxyPlot.Axes;
using Prism.Regions;
using SharedUI.BasicPlot;

namespace Training.Application.ViewModels
{
    public class ErrorPlotSettingsViewModel : PlotEpochParametersViewModel
    {
        private bool _logarithmicScale;
        private BasicPlotModel? _plotModel;

        public bool LogarithmicScale
        {
            get => _logarithmicScale;
            set
            {
                SetProperty(ref _logarithmicScale, value);

                if (value)
                {
                    _plotModel!.Model.Axes[1] = new LinearAxis()
                    {
                        Position = AxisPosition.Left,
                        Title = "Error",
                        AbsoluteMinimum = 0,
                        AxisTitleDistance = 18,
                    };
                }
                else
                {
                    _plotModel!.Model.Axes[1] = new LogarithmicAxis()
                    {
                        Position = AxisPosition.Left,
                        Title = "Error",
                        AbsoluteMinimum = 0,
                        AxisTitleDistance = 18,
                    };
                }
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            _plotModel = navigationContext.Parameters["PlotModel"] as BasicPlotModel;
            Debug.Assert(_plotModel != null);
        }
    }
}
