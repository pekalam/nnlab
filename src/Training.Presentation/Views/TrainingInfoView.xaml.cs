using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;
using Training.Application.ViewModels;
using Training.Application.Views;

namespace Training.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TrainingInfoView
    /// </summary>
    public partial class TrainingInfoView : UserControl, ITrainingInfoView
    {
        public TrainingInfoView()
        {
            InitializeComponent();
            (DataContext as TrainingInfoViewModel)!.SetView(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        public void UpdateTimer(in TimeSpan timeSpan)
        {
            var txt = ToTimerString(timeSpan);
            Dispatcher.InvokeAsync(() => TimerText.Text = txt, DispatcherPriority.Normal);
        }

        public void UpdateTraining(double error, int epochs, int iterations, double? validationError)
        {
            var e = error.ToString(CultureInfo.InvariantCulture);
            var ep = epochs.ToString();
            var it = iterations.ToString();
            var val = validationError?.ToString(CultureInfo.InvariantCulture);
            Dispatcher.InvokeAsync(() =>
            {
                Error.Text = e;
                Epoch.Text = ep;
                Iterations.Text = it;
                ValidationError.Text = val;
            }, DispatcherPriority.Render);
        }

        public void ResetProgress()
        {
            TimerText.Text = "-";
            Error.Text = "-";
            Epoch.Text = "-";
            Iterations.Text = "-";
            ValidationError.Text = null;
        }
    }
}