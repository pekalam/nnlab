using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Common.Domain;

namespace Shell.Presentation.Views
{
    /// <summary>
    /// Interaction logic for StatusBarView
    /// </summary>
    public partial class StatusBarView : UserControl
    {
        private List<string> _initialNames = new List<string>();
        private ObservableCollection<Session>? _sessions;

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

        private void SessionNames_OnDropDownOpened(object? sender, EventArgs e)
        {
            _sessions = (SessionNames.ItemsSource as ObservableCollection<Session>)!;
            _initialNames = _sessions.Select(c => c.Name).ToList();
        }

        private void SessionNameBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var txt = (sender as TextBox)!;
            txt.IsReadOnly = true;
            if (Validation.GetErrors(txt).Count == 0)
            {
                return;
            }

            for (int i = 0; i < _sessions!.Count; i++)
            {
                if (_sessions[i] == txt.Tag)
                {
                    txt.Text = _initialNames[i];
                    break;
                }
            }

            txt.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
        }

        private void SessionNameBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (sender as TextBox)!.IsReadOnly = false;
        }
    }
}
