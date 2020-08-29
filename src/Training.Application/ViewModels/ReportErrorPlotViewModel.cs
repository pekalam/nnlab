using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Common.Framework.Extensions;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Regions;
using SharedUI.BasicPlot;
using Training.Application.Services;

namespace Training.Application.ViewModels
{
    class ReportErrorPlotNavParams : NavigationParameters
    {
        public ReportErrorPlotNavParams(string parentRegion, List<DataPoint> points)
        {
            Add(nameof(ReportErrorPlotRecNavParams.ParentRegion), parentRegion);
            Add(nameof(ReportErrorPlotRecNavParams.Points), points);
        }

        public class ReportErrorPlotRecNavParams
        {
            private readonly NavigationParameters _parameters;

            public ReportErrorPlotRecNavParams(NavigationParameters parameters)
            {
                _parameters = parameters;
            }

            public string ParentRegion => _parameters.GetOrDefault<string>(nameof(ParentRegion));
            public List<DataPoint> Points => _parameters.GetOrDefault<List<DataPoint>>(nameof(Points));
        }

        public static ReportErrorPlotRecNavParams FromParams(NavigationParameters navParams)
        {
            return new ReportErrorPlotRecNavParams(navParams);
        }
    }

    public class ReportErrorPlotViewModel : ViewModelBase<ReportErrorPlotViewModel>
    {
        public ReportErrorPlotViewModel(IReportErrorService service)
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
            BasicPlotModel.Model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "Error",
                AbsoluteMinimum = 0,
                AxisTitleDistance = 18,
            });
            BasicPlotModel.Model.PlotMargins = new OxyThickness(80, Double.NaN, Double.NaN, Double.NaN);
        }

        public IReportErrorService Service { get; }

        public LineSeries Series = new LineSeries();

        public BasicPlotModel BasicPlotModel { get; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}
