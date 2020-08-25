using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Training.Application.ViewModels;

namespace Training.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TrainingInfoView
    /// </summary>
    public partial class TrainingInfoView : UserControl
    {
        public TrainingInfoView()
        {
            InitializeComponent();
        }

        private string ToTimerString(in TimeSpan date)
        {
            var str = new StringBuilder();

            if (date.TotalMilliseconds < 0)
            {
                return "00:00:00";
            }

            str.Append(date.Days > 0 ? date.Days + " d " : "");
            str.Append(date.Hours < 10 ? "0" + date.Hours : date.Hours.ToString());
            str.Append(":");
            str.Append(date.Minutes < 10 ? "0" + date.Minutes : date.Minutes.ToString());
            str.Append(":");
            str.Append(date.Seconds < 10 ? "0" + date.Seconds : date.Seconds.ToString());
            return str.ToString();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            switch (e.Property.Name)
            {
                case nameof(DataContext):
                    if (DataContext is TrainingInfoViewModel vm)
                    {
                        vm.UpdateTimer = span =>
                        {
                            var txt = ToTimerString(span);
                            Dispatcher.InvokeAsync(() => TimerText.Text = txt, DispatcherPriority.Normal);
                        };
                        vm.UpdateTraining = (error, epoch, iterations) =>
                        {
                            var e = error.ToString();
                            var ep = epoch.ToString();
                            var it = iterations.ToString();
                            Dispatcher.InvokeAsync(() =>
                            {
                                Error.Text = e;
                                Epoch.Text = ep;
                                Iterations.Text = it;
                            }, DispatcherPriority.Normal);
                        };
                    }
                    break;
            }
        }
    }
}
