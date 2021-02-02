using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Training.Application.ViewModels;

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

        private void ClearColorsMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (DataContext is TrainingNetworkPreviewViewModel vm)
            {
                vm.Service.ClearColorsCommand.Execute();
            }
        }

        private void ToggleAnimationMenuItem_OnClick(object sender, RoutedEventArgs e)
        { 
            if(DataContext is TrainingNetworkPreviewViewModel vm)
            {
                vm.Service.ToggleAnimationCommand.Execute();
            }
        }
    }
}
