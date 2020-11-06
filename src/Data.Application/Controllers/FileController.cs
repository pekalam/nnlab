using Common.Domain;
using Common.Framework;
using Data.Application.Interfaces;
using Data.Application.Services;
using Data.Application.ViewModels;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;

namespace Data.Application.Controllers
{
    internal interface IFileController : ITransientController, IFileService
    {
        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IFileController, FileController>();
        }
    }

    internal class FileController : ControllerBase<FileDataSourceViewModel>,IFileController
    {
        private readonly IRegionManager _rm;
        private readonly IFileDialogService _fileDialogService;
        private readonly AppState _appState;

        public FileController(IRegionManager rm, IFileDialogService fileDialogService, AppState appState)
        {
            _rm = rm;
            _fileDialogService = fileDialogService;
            _appState = appState;

            CreateDataSetCommand = new DelegateCommand(CreateDataSet);
            SelectFileCommand = new DelegateCommand(SelectFile);
            LoadFilesCommand = new DelegateCommand(SelectFiles);
        }

        public void Initialize() { }

        private void SelectFiles()
        {
            _rm.NavigateContentRegion("MultiFileSourceView");
        }


        private void SelectFile()
        {
            var dialog = _fileDialogService.OpenCsv();
            if (dialog.result == true)
            {
                _rm.NavigateContentRegion("SingleFileSourceView");
                SingleFileSourceViewModel.Instance!.SelectedFilePath = dialog.filePath;
            }
        }

        private void CreateDataSet()
        {
            if (_appState.Sessions.Count == 0)
            {
                _appState.CreateSession();
            }
            _rm.NavigateContentRegion("CustomDataSetView");
        }

        public DelegateCommand CreateDataSetCommand { get; set; }
        public DelegateCommand SelectFileCommand { get; set; }
        public DelegateCommand LoadFilesCommand { get; set; }
    }
}
