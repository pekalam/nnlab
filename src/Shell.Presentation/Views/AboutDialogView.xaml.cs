using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Services.Dialogs;

namespace Shell.Presentation.Views
{
    public class AboutDialogViewModel : IDialogAware
    {

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public string Title { get; } = "About";
        public event Action<IDialogResult> RequestClose;
    }

    /// <summary>
    /// Interaction logic for AboutDialogView.xaml
    /// </summary>
    public partial class AboutDialogView : UserControl 
    {
        public AboutDialogView()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri){UseShellExecute = true});
        }
    }
}
