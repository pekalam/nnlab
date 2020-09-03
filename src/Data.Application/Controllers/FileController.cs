using Common.Domain;
using Common.Framework;
using Data.Application.Interfaces;
using Data.Application.Services;
using Data.Application.ViewModels.CustomDataSet;
using Data.Application.ViewModels.DataSourceSelection;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;

namespace Data.Application.Controllers
{
    internal interface IFileController : ISingletonController
    {
        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IFileController, FileController>();
        }
    }

    internal class FileController : IFileController
    {
        private readonly IRegionManager _rm;
        private readonly FileService _fileService;
        private readonly IFileDialogService _fileDialogService;
        private readonly AppState _appState;

        public FileController(IRegionManager rm, FileService fileService, IFileDialogService fileDialogService, AppState appState)
        {
            _rm = rm;
            _fileService = fileService;
            _fileDialogService = fileDialogService;
            _appState = appState;

            _fileService.CreateDataSetCommand = new DelegateCommand(CreateDataSet);
            _fileService.SelectFileCommand = new DelegateCommand(SelectFile);
            _fileService.LoadFilesCommand = new DelegateCommand(SelectFiles);
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
    }
}
