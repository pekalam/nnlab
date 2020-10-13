using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Prism.Mvvm;
using SharedUI.BasicPlot;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class VariablesPlotViewModel : BindableBase
    {
        public VariablesPlotViewModel()
        {
            PlotModel.Model.Title = "Variables plot";
            PlotModel.Model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "n-th variable",
                MinimumMajorStep = 1,
                MinimumMinorStep = 1,
            });
            PlotModel.Model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "Value",
            });

            var l = new Legend
            {
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendPosition = LegendPosition.RightTop,
                LegendPlacement = LegendPlacement.Inside,
            };

            PlotModel.Model.Legends.Add(l);


            PlotModel.Model.IsLegendVisible = true;
            PlotModel.DisplaySettingsRegion = false;
            Controller = new VariablesPlotController(this);
        }

        public VariablesPlotController Controller { get; }

        public BasicPlotModel PlotModel { get; set; } = new BasicPlotModel();
    }
}