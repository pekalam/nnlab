using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace Data.Presentation.Services
{
    public interface IFileDialogService
    {
        (bool? result, string filePath) OpenCsv(Window owner);
    }

    public class FileDialogService : IFileDialogService {

        public (bool? result, string filePath) OpenCsv(Window owner)
        {
            var dialog = new OpenFileDialog()
            {
                DefaultExt = ".csv",
                Multiselect = false,
                Filter = "Csv files (*.csv)|*.csv",
            };

            var result = dialog.ShowDialog();

            if (result == true)
            {
                return (true, dialog.FileName);
            }

            return (result, null);
        }
    }
}
