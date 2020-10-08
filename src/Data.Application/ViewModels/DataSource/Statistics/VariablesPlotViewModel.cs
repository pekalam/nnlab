using OxyPlot;
using OxyPlot.Axes;
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
            PlotModel.Model.IsLegendVisible = true;
            PlotModel.DisplaySettingsRegion = false;
            Controller = new VariablesPlotController(this);
        }

        public VariablesPlotController Controller { get; }

        public BasicPlotModel PlotModel { get; set; } = new BasicPlotModel();
    }
}