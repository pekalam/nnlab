using System.ComponentModel;
using System.Windows;

namespace SharedUI.BasicPlot
{
    /// <summary>
    /// Interaction logic for ExternalWindow.xaml
    /// </summary>
    public partial class ExternalWindow : Window
    {

        public ExternalWindow(BasicPlotModel model)
        {
            Model = model;
            InitializeComponent();
            DataContext = this;
        }

        public BasicPlotModel Model { 
            get; 
            set;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            plot.PlotModel = null;
            base.OnClosing(e);
        }
    }
}
