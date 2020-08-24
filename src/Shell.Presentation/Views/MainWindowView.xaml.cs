using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using MahApps.Metro.Controls;

namespace Shell.Presentation.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : MetroWindow
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        public void ShowOverlay(bool show)
        {
            System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {

               

            }, DispatcherPriority.Send);


        }
    }
}
