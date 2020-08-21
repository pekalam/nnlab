using Common.Framework;
using Data.Application.Services;
using Infrastructure;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace Data.Application.ViewModels.CustomDataSet
{
    public class CustomDataSetViewModel : ViewModelBase<CustomDataSetViewModel>
    {
        public CustomDataSetViewModel(ICustomDataSetService customDataSetService)
        {
            Service = customDataSetService;

            PlotModel.MouseDown += (sender, args) => Service.PlotMouseDownCommand.Execute(args);

            PlotModel.Series.Add(Scatter);
            PlotModel.Series.Add(Line);
        }

        public ICustomDataSetService Service { get; set; }


        public ScatterSeries Scatter { get; } = new ScatterSeries()
        {
            MarkerType = MarkerType.Circle,
            MarkerFill = OxyColor.FromRgb(0, 0, 255),
        };
        public LineSeries Line { get; } = new LineSeries()
        {
            Color = OxyColor.FromRgb(0, 255, 0),
        };

        public PlotModel PlotModel { get; set; } = new PlotModel()
        {
            Axes =
            {
                new LinearAxis()
                {
                    Title = "x",
                    Position = AxisPosition.Bottom,
                    MajorGridlineStyle = LineStyle.Solid,
                },
                new LinearAxis()
                {
                    Title = "y",
                    Position = AxisPosition.Left,
                    MajorGridlineStyle = LineStyle.Solid,
                }
            }
        };

        public PlotController PlotController { get; } = new PlotController();
    }
}