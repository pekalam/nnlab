using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using Prism.Mvvm;
using SharedUI.BasicPlot;

namespace Data.Application.ViewModels
{
    public enum VariablePlotType{Input,Target}

    public class VariablesPlotViewModel : BindableBase
    {
        private VariablePlotType _selectedVariablePlotType;

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

        public VariablePlotType[] VariablePlotTypes { get; } = new[] {VariablePlotType.Input, VariablePlotType.Target};

        public VariablePlotType SelectedVariablePlotType
        {
            get => _selectedVariablePlotType;
            set => SetProperty(ref _selectedVariablePlotType, value);
        }

        public BasicPlotModel PlotModel { get; set; } = new BasicPlotModel();
    }
}