using Common.Framework;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Windows;

namespace Common.Presentation
{
    internal class MetroDialogService : IMetroDialogService
    {
        public bool ShowModalConfirmationDialog(string title, string message)
        {
            var window = Application.Current?.MainWindow as MetroWindow;

            if (window == null) return false;

           return window.ShowModalMessageExternal(title, message, MessageDialogStyle.AffirmativeAndNegative) == MessageDialogResult.Affirmative;
        }
    }
}
