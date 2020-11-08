using CommonServiceLocator;
using Prism.Regions;
using Prism.Regions.Behaviors;
using Prism.Services.Dialogs;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Training.Application;
using Training.Application.ViewModels;

namespace Training.Presentation.Views.PanelLayout
{
    /// <summary>
    /// Interaction logic for PanelContainer.xaml
    /// </summary>
    public partial class PanelContainer : UserControl
    {
        public static readonly DependencyProperty DisplayedPanelProperty = DependencyProperty.Register(
           "DisplayedPanel", typeof(PanelSelectModel), typeof(PanelContainer),
           new PropertyMetadata(default(PanelSelectModel)));

        public PanelSelectModel DisplayedPanel
        {
            get { return (PanelSelectModel)GetValue(DisplayedPanelProperty); }
            set { SetValue(DisplayedPanelProperty, value); }
        }

        public static readonly DependencyProperty RegionProperty = DependencyProperty.Register(
            "Region", typeof(string), typeof(PanelContainer),
            new PropertyMetadata(default(string), PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var container = (d as PanelContainer)!;
            var regName = (e.NewValue as string)!;

            var rm = ServiceLocator.Current.GetInstance<IRegionManager>();
            if (rm.Regions.ContainsRegionWithName(regName))
            {
                rm.Regions.Remove(regName);
            }

            RegionManager.SetRegionName(container.region, regName);
            RegionManager.SetRegionManager(container.region, rm);
            container.region.SetValue(ClearChildViewsRegionBehavior.ClearChildViewsProperty, true);
        }

        public string Region
        {
            get { return (string)GetValue(RegionProperty); }
            set { SetValue(RegionProperty, value); }
        }

        public PanelContainer()
        {
            InitializeComponent();
        }

        private void ChangePanelExecute()
        {
            var ds = ServiceLocator.Current.GetInstance<IDialogService>();

            var parameters = new DialogParameters();
            parameters.Add("single", true);
            parameters.Add("selected", DisplayedPanel.PanelType);
            parameters.Add(nameof(PanelSelectionResult),
                new PanelSelectionResult(list =>
                {
                    DisplayedPanel = list.First();
                }));
            ds.ShowDialog(nameof(PanelSelectView), parameters, null);
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ChangePanelExecute();
            }
        }
    }
}
