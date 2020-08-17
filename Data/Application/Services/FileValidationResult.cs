using Prism.Mvvm;

namespace Data.Application.Services
{
    public class FileValidationResult : BindableBase
    {
        private bool _isLoadingFile;
        private bool _isValidatingFile;
        private int _rows;
        private int _cols;
        private bool? _isFileValid;
        private string _fileValidationError;

        public bool IsLoadingFile
        {
            get => _isLoadingFile;
            set => SetProperty(ref _isLoadingFile, value);
        }

        public bool IsValidatingFile
        {
            get => _isValidatingFile;
            set => SetProperty(ref _isValidatingFile, value);
        }

        public bool? IsFileValid
        {
            get => _isFileValid;
            set => SetProperty(ref _isFileValid, value);
        }

        public string FileValidationError
        {
            get => _fileValidationError;
            set => SetProperty(ref _fileValidationError, value);
        }

        public int Rows
        {
            get => _rows;
            set => SetProperty(ref _rows, value);
        }

        public int Cols
        {
            get => _cols;
            set => SetProperty(ref _cols, value);
        }
    }
}