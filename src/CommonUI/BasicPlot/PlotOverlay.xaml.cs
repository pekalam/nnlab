using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SharedUI.BasicPlot
{
    /// <summary>
    /// Interaction logic for PlotOverlay.xaml
    /// </summary>
    public partial class PlotOverlay : UserControl, INotifyPropertyChanged
    {
        public PlotOverlay()
        {
            InitializeComponent();
            DataContext = this;
        }

        public static readonly DependencyProperty PlotModelProperty = DependencyProperty.Register(
            "PlotModel", typeof(BasicPlotModel), typeof(PlotOverlay), new PropertyMetadata(default(BasicPlotModel)));

        public Action OnNewWindowClicked;
        public Action OnSettingsClicked;
        public Action OnAsPhotoClicked;
        private Visibility _newWindowVisibility;
        private Visibility _settingsVisibility;

        public Visibility NewWindowVisibility
        {
            get => _newWindowVisibility;
            set
            {
                _newWindowVisibility = value;
                OnPropertyChanged(nameof(NewWindowVisibility));
            }
        }

        public Visibility SettingsVisibility
        {
            get => _settingsVisibility;
            set
            {
                _settingsVisibility = value;
                OnPropertyChanged(nameof(SettingsVisibility));
            }
        }

        public BasicPlotModel PlotModel
        {
            get => (BasicPlotModel) GetValue(PlotModelProperty);
            set => SetValue(PlotModelProperty, value);
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            PlotModel.Model.DefaultXAxis.Reset();
            PlotModel.Model.DefaultYAxis.Reset();
            PlotModel.Model.InvalidatePlot(false);
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            PlotModel.Model.ZoomAllAxes(1.1);
            PlotModel.Model.InvalidatePlot(false);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            PlotModel.Model.ZoomAllAxes(0.9);
            PlotModel.Model.InvalidatePlot(false);
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            OnSettingsClicked?.Invoke();
        }

        private void NewWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            OnNewWindowClicked?.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AsPhoto_Click(object sender, RoutedEventArgs e)
        {
            OnAsPhotoClicked?.Invoke();
        }
    }
}
