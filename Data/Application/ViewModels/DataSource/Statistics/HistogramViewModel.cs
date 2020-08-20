using OxyPlot;
using OxyPlot.Axes;
using Prism.Mvvm;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class HistogramViewModel : BindableBase
    {
        private double _binWidth = 4;

        public HistogramViewModel()
        {
            HistogramModel = new PlotModel()
            {
                Title = "Histogram"
            };
            HistogramModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Frequency", MinorTickSize = 0 });
            HistogramModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "x" });
        }

        public PlotModel HistogramModel { get; set; }

        public double BinWidth
        {
            get => _binWidth;
            set => SetProperty(ref _binWidth, value);
        }
    }
}