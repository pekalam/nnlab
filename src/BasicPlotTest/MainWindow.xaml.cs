using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SharedUI.BasicPlot;
using System;
using System.Windows;

namespace BasicPlotTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            
            Model.Model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Epoch",
                AbsoluteMinimum = 0,
                AxisTitleDistance = 18,
            });
            Model.Model.Axes.Add(new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "Error",
                AbsoluteMinimum = 0,
                AxisTitleDistance = 18,
            });
            Model.Model.Series.Add(new LineSeries()
            {
                Points = { new DataPoint(0, 0), new DataPoint(1, 1), new DataPoint(2, 4000_000) }
            });
        }

        public BasicPlotModel Model { get; set; } = new BasicPlotModel()
        {
            Model = new PlotModel()
            {
                Background = OxyColor.Parse("#F0F0F0"),
                PlotAreaBackground = OxyColor.Parse("#FFFFFF"),
                PlotMargins = new OxyThickness(80, Double.NaN, Double.NaN, Double.NaN),
            },
            Controller = new PlotController(),
            DisplaySettingsRegion = false,
        };
    }
}