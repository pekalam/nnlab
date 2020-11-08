using CommonServiceLocator;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Wpf;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SharedUI.BasicPlot
{
    public class BooleanHiddenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = System.Convert.ToBoolean(value);

            return b ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Interaction logic for BasicPlot.xaml
    /// </summary>
    public partial class BasicPlot : UserControl
    {
        private static readonly (int w, int h)[] ImgSz = new (int w, int h)[]
        {
            (600,400),(1200,900)
        };

        public static readonly DependencyProperty PlotModelProperty = DependencyProperty.Register(
            nameof(OxyPlot.PlotModel), typeof(BasicPlotModel), typeof(BasicPlot),
            new FrameworkPropertyMetadata(default(BasicPlotModel), FrameworkPropertyMetadataOptions.AffectsRender,
                (o, args) =>
                {
                    if (args.NewValue != null)
                        ((BasicPlot) o).OnModelChanged((args.NewValue as BasicPlotModel)!);
                }));

        public BasicPlotModel? PlotModel
        {
            get => (BasicPlotModel) GetValue(PlotModelProperty);
            set => SetValue(PlotModelProperty, value);
        }

        public static readonly DependencyProperty WpfProperty = DependencyProperty.Register(
            "Wpf", typeof(bool), typeof(BasicPlot), new PropertyMetadata(default(bool)));

        public bool Wpf
        {
            get => (bool) GetValue(WpfProperty);
            set => SetValue(WpfProperty, value);
        }

        public BasicPlot()
        {
            InitializeComponent();
            plotOverlay.OnNewWindowClicked = OnNewWindowClicked;
            plotOverlay.OnSettingsClicked = OnSettingsClicked;
            plotOverlay.OnAsPhotoClicked = OnAsPhotoClicked;
        }

        private PlotViewBase Plot => (PlotContainer.Content as PlotViewBase)!;

        private void OnAsPhotoClicked()
        {
            Debug.Assert(PlotModel != null, nameof(PlotModel) + " != null");

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = saveFileDialog.InitialDirectory + PlotModel.DefaultSaveName;
            saveFileDialog.Filter = "Png 600x400 (*.png)|*.png|Png 1200x900 (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                var stream = File.OpenWrite(saveFileDialog.FileName);
                var pngExporter = new OxyPlot.Wpf.PngExporter { Width = ImgSz[saveFileDialog.FilterIndex].w, Height = ImgSz[saveFileDialog.FilterIndex].h, /*Background = PlotModel.Model.Background*/ };
                pngExporter.Export(PlotModel.Model, stream);
                stream.Close();
            }



        }

        private void OnSettingsClicked()
        {
            SettingsContainer.Visibility = SettingsContainer.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (SettingsContainer.Visibility == Visibility.Collapsed)
            {
                BindingOperations.SetBinding(plotOverlay, TextBlock.VisibilityProperty, new Binding()
                {
                    ElementName = "root",Converter = new BooleanHiddenVisibilityConverter(),Path = new PropertyPath("IsMouseOver"),
                });
            }
            else
            {
                BindingOperations.ClearBinding(plotOverlay,TextBlock.VisibilityProperty);
            }
        }

        private void OnModelChanged(BasicPlotModel model)
        {
            plotOverlay.NewWindowVisibility = model.DisplayNewWindow ? Visibility.Visible : Visibility.Collapsed;
            plotOverlay.SettingsVisibility = model.DisplayNewWindow ? Visibility.Visible : Visibility.Collapsed;
            model.SetSettingsRegion = name =>
            {
                var rm = ServiceLocator.Current.GetInstance<IRegionManager>();
                if (rm.Regions.ContainsRegionWithName(name))
                {
                    rm.Regions.Remove(name);
                }

                RegionManager.SetRegionName(SettingsContent, name);
                RegionManager.SetRegionManager(SettingsContent, rm);
            };
            //TODO use
            model.RemoveSettingsRegion = name =>
            {
                var rm = ServiceLocator.Current.GetInstance<IRegionManager>();
                rm.Regions.Remove(name);
            };
            model.Controller.BindMouseDown(OxyMouseButton.Left, OxyModifierKeys.None, new DelegatePlotCommand<OxyMouseDownEventArgs>(
                (view, controller, arg3) =>
                {
                    if (SettingsContainer.Visibility == Visibility.Visible)
                    {
                        OnSettingsClicked();
                    }
                }));
        }

        private void OnNewWindowClicked()
        {
            var model = PlotModel;
            if (model == null)
            {
                return;
            }

            model.DisplayNewWindow = false;
            PlotModel = null;
            Plot.Visibility = Visibility.Hidden;
            plotPlaceholder.Visibility = Visibility.Visible;


            ExternalWindow w = new ExternalWindow(model);
            w.Show();

            w.Closed += (sender2, e2) => { OnExternalPlotModelClosed(model); };
        }

        private void OnExternalPlotModelClosed(BasicPlotModel model)
        {
            Plot.Visibility = Visibility.Visible;
            plotPlaceholder.Visibility = Visibility.Collapsed;
            model.DisplayNewWindow = true;
            PlotModel = model;
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (SettingsContainer.Visibility == Visibility.Visible && e.Source != SettingsContainer)
            {
                OnSettingsClicked();
            }

        }
    }

    public class BasicPlotModel : BindableBase
    { 
        public PlotModel Model { get; set; } = new PlotModel()
        {
            Background = OxyColor.Parse("#F0F0F0"),
            PlotAreaBackground = OxyColor.Parse("#FFFFFF"),
        };

        public string DefaultSaveName { get; set; } = "Plot";
        public PlotController Controller { get; set; } = new PlotController();
        internal bool DisplayNewWindow { get; set; } = true;

        public Action<string>? SetSettingsRegion { get; internal set; }
        public Action<string>? RemoveSettingsRegion { get; internal set; }
        public bool DisplaySettingsRegion { get; set; } = true;
    }
}