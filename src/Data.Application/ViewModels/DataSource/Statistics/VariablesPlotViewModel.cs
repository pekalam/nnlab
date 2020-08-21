using OxyPlot;
using Prism.Mvvm;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class VariablesPlotViewModel : BindableBase
    {
        public VariablesPlotViewModel()
        {
            PlotModel = new PlotModel()
            {
                Title = "Plot"
            };
            Controller = new VariablesPlotController(this);
        }

        public PlotModel PlotModel { get; set; }
        public VariablesPlotController Controller { get; }
    }
}