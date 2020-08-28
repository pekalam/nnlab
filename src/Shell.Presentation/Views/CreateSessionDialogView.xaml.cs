using System.Windows.Controls;

namespace Shell.Presentation.Views
{
    /// <summary>
    /// Interaction logic for CreateSessionDialogView
    /// </summary>
    public partial class CreateSessionDialogView : UserControl
    {
        public CreateSessionDialogView()
        {
            Loaded += (sender, args) => SessionName.Focus();
            InitializeComponent();
        }
    }
}
