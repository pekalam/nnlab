using MahApps.Metro.Controls;
using System.Windows.Threading;

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
