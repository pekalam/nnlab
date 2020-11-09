using Common.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Shell.Presentation.Views
{
    /// <summary>
    /// Interaction logic for StatusBarView
    /// </summary>
    public partial class StatusBarView : UserControl
    {

        private List<string> _initialNames = new List<string>();
        private ObservableCollection<Session>? _sessions;

        private Timer _doubleTimer;
        private int _clicks;
        private object _clickedDataContext;
        private object _clickedSender;

        public StatusBarView()
        {
            _doubleTimer = new Timer(250);
            _doubleTimer.Elapsed += DoubleTimerOnElapsed;
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
        private void DoubleTimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _doubleTimer.Stop();

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (_clicks == 1)
                {
                    SessionNames.SelectedItem = _clickedDataContext;
                }
                else if (_clicks == 2)
                {
                    (_clickedSender as TextBox)!.IsReadOnly = false;
                }
            });

            _clicks = 0;
        }

        private void SessionNameBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _doubleTimer.Stop();

            _clickedDataContext = (VisualTreeHelper.GetParent((sender as TextBox)!) as ContentPresenter).DataContext;
            _clickedSender = sender;
            _clicks++;

            _doubleTimer.Start();
        }
    }
}
