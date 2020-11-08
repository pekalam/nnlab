using Data.Application.Interfaces;
using Microsoft.Win32;

namespace Data.Presentation.Services
{
    internal class FileDialogService : IFileDialogService {

        public (bool? result, string? filePath) OpenCsv()
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
