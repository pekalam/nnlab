using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Data.Application;
using Data.Application.Interfaces;
using Data.Presentation.Views;
using Data.Presentation.Views.CustomDataSet;
using Data.Presentation.Views.DataSetDivision;
using Data.Presentation.Views.DataSource.FileDataSource;
using Data.Presentation.Views.DataSource.VariablesSelection;
using Microsoft.Win32;
using Prism.Regions;
using Shell.Interface;

namespace Data.Presentation.Services
{
    internal class FileDialogService : IFileDialogService {

        public (bool? result, string filePath) OpenCsv()
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
