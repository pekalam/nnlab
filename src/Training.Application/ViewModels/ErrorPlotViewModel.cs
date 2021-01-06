using Common.Framework;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Regions;
using SharedUI.BasicPlot;
using System;
using OxyPlot.Legends;
using Training.Application.Controllers;

namespace Training.Application.ViewModels
{
    public class ErrorPlotViewModel : ViewModelBase<ErrorPlotViewModel>
    {
        public ErrorPlotViewModel(IErrorPlotController service)
        {
            Service = service;
            BasicPlotModel = new BasicPlotModel();
            BasicPlotModel.Model.Series.Add(ErrorSeries);
            BasicPlotModel.Model.Series.Add(ValidationSeries);
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
                AxisTitleDistance = 18,
                MinimumRange = 0.001,
            };

            BasicPlotModel.Model.Axes.Add(yAxis);
            BasicPlotModel.Model.PlotMargins = new OxyThickness(80, Double.NaN, Double.NaN, Double.NaN);
            BasicPlotModel.Model.Legends.Add(new Legend {IsLegendVisible = false});
            service.Initialize(this);
        }

        public IErrorPlotController Service { get; }

        public LineSeries ErrorSeries = new LineSeries()
        {
            Title = "Training error",
        };
        
        public LineSeries ValidationSeries = new LineSeries()
        {
            Color = OxyColors.Blue,
            Title = "Validation error",
            IsVisible = false,
        };

        public BasicPlotModel BasicPlotModel { get; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}
