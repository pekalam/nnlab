using System.Collections.Generic;
using System.Windows;
using CommonServiceLocator;
using Prism.Regions;
using Training.Application;
using Training.Application.ViewModels.PanelLayout;
using Training.Presentation.Views.PanelLayout;

namespace PanelLayoutTest.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int i = 0;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            i = (i + 1) % 2;
            var rm = ServiceLocator.Current.GetInstance<IRegionManager>();

            rm.Regions["ContentRegion"].RemoveAll();
            switch (i)
            {
                case 0:
                    rm.Regions["ContentRegion"].RequestNavigate(nameof(PanelLayoutView), new PanelLayoutNavigationParams(new List<PanelSelectModel>()
                    {
                        new PanelSelectModel()
                        {
                            PanelType = Panels.MatrixPreview,
                        }
                    }));
                    break;
                case 1:
                    rm.Regions["ContentRegion"].RequestNavigate(nameof(PanelLayoutView), new PanelLayoutNavigationParams(new List<PanelSelectModel>()
                    {
                        new PanelSelectModel() {PanelType = Panels.MatrixPreview},
                        new PanelSelectModel(){PanelType = Panels.NetworkVisualization},
                    }));
                    break;
            }
        }
    }
}
