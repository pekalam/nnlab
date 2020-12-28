using System.Windows.Controls;
using System.Windows.Input;

namespace Training.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TrainingNetworkPreviewView
    /// </summary>
    public partial class TrainingNetworkPreviewView : UserControl
    {
        public TrainingNetworkPreviewView()
        {
            InitializeComponent();
        }

        private void NetworkControl_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (e.ChangedButton == MouseButton.Right)
                {
                    NetworkControl.ContextMenu.IsOpen = true;
                }
                else if (e.ChangedButton == MouseButton.Left)
                {
                    e.Handled = true;
                }
            }
        }
    }
}
