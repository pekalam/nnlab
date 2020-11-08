using System.Windows.Controls;
using System.Windows.Input;

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

        private void SessionName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OkButton.Command.Execute(null);
            }
        }
    }
}
