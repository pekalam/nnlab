using Common.Domain;
using OxyPlot;
using OxyPlot.Axes;
using Prism.Mvvm;
using SharedUI.BasicPlot;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class HistogramViewModel : BindableBase
    {
        private double _binWidth = 4;
        private string[]? _variables;
        private string? _selectedVariable;

        public HistogramViewModel()
        {
            HistogramModel.Model.Title = "Histogram";
            HistogramModel.DisplaySettingsRegion = false;
            HistogramModel.Model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "Frequency", MinorTickSize = 0 });
            HistogramModel.Model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "x" });
        }

        public BasicPlotModel HistogramModel { get; set; } = new BasicPlotModel(){Wpf = true};

        public string[]? Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value);
        }

        public string? SelectedVariable
        {
            get => _selectedVariable;
            set => SetProperty(ref _selectedVariable, value);
        }

        public double BinWidth
        {
            get => _binWidth;
            set => SetProperty(ref _binWidth, value);
        }
    }
}