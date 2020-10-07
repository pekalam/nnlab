using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using System.Windows;
using Common.Framework;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

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
