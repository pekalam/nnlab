using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Shell.Presentation.Views
{
    /// <summary>
    /// Interaction logic for StatusBarView
    /// </summary>
    public partial class StatusBarView : UserControl
    {
        public StatusBarView()
        {
            InitializeComponent();
        }

        private void ProgressArea_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                (Resources["CogAnimation"] as Storyboard)!.Begin();
            }
            else
            {
                (Resources["CogAnimation"] as Storyboard)!.Stop();
            }
        }
    }
}
