using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Regions;
using SharedUI.BasicPlot;
using Training.Application.Services;

namespace Training.Application.ViewModels
{
    public class ErrorPlotViewModel : ViewModelBase<ErrorPlotViewModel>
    {

        public static Func<double, string> CreateDefaultLabelFormatter(Axis yAxis)
        {
            return d =>
            {
                var i = 0;
                var v = 1d;

                var x = yAxis.ActualMaximum - yAxis.ActualMinimum;

                while ((v /= 10d) > x)
                {
                    i++;
                }

                i++;

                if (i <= 4 || i > 10)
                {
                    return d.ToString("g6");
                }

                return d.ToString("F" + i);
            };
        }

        public ErrorPlotViewModel(IErrorPlotService service)
        {
            Service = service;
            BasicPlotModel = new BasicPlotModel();
            BasicPlotModel.Model.Series.Add(Series);
            BasicPlotModel.Model.Title = "Network error";
            BasicPlotModel.Model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Epoch",
                AbsoluteMinimum = 0,
                AxisTitleDistance = 4,
            });
            var yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "Error",
                AbsoluteMinimum = 0,
                AxisTitleDistance = 18,
            };
            yAxis.LabelFormatter = CreateDefaultLabelFormatter(yAxis);

            BasicPlotModel.Model.Axes.Add(yAxis);
            BasicPlotModel.Model.PlotMargins = new OxyThickness(80, Double.NaN, Double.NaN, Double.NaN);
        }

        public IErrorPlotService Service { get; }

        public LineSeries Series = new LineSeries();

        public BasicPlotModel BasicPlotModel { get; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}
