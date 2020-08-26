using System.Collections.Generic;
using Common.Domain;
using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Interfaces;
using Data.Application.Services;
using Data.Application.ViewModels.DataSourceSelection;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Commands;
using Prism.Regions;
using TestUtils;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Data.Application.Tests.DataSourceSelection
{
    public class FileSelectTest
    {
        private readonly AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private SingleFileService _singleFileService;
        private MultiFileService _multiFileService;
        private Mock<FileService> _fileService;
        private Mock<IFileDialogService> _dialogService;
        private Mock<IRegionManager> _rm;
        private Dictionary<string, Mock<IRegion>> _regions;

        private FileController _ctrl;

        public FileSelectTest()
        {
            (_rm, _regions) = _mocker.UseTestRm();
            _appState = _mocker.UseImpl<AppState>();
            _dialogService = _mocker.UseMock<IFileDialogService>();
            _fileService = _mocker.UseMock<IFileService, FileService>();
            _mocker.UseImpl<ITransientController<SingleFileService>, SingleFileSourceController>();
            _singleFileService = _mocker.UseImpl<ISingleFileService, SingleFileService>();
            _mocker.UseImpl<ITransientController<MultiFileService>,MultiFileSourceController>();
            _multiFileService = _mocker.UseImpl<IMultiFileService, MultiFileService>();


            _ctrl= _mocker.CreateInstance<FileController>();
        }

        [Fact]
        public void SelectFileCommand_when_dialog_returns_path_navigates_to_next_view()
        {
            //arrange
            var vm = _mocker.CreateInstance<SelectDataSourceViewModel>(); //current vm
            var sigVm = _mocker.CreateInstance<SingleFileSourceViewModel>(); //next view vm

            var successfulPath = "C:\\Dir\\test.csv";

            //stop validation execution
            _singleFileService.ValidateCommand = new DelegateCommand<string>(s => { }, _ => false);
            //setup dialog success
            _dialogService.Setup(f => f.OpenCsv()).Returns((true, successfulPath));

            //act
            vm.FileService.SelectFileCommand.Execute();


            //assert
            _rm.VerifyContentNavigation("SingleFileSourceView", Times.Exactly(1));


            sigVm.SelectedFilePath.Should().Be(successfulPath);
            sigVm.SelectedFileName.Should().Be("test.csv");
        }


        [Fact]
        public void SelectFileCommand_when_dialog_returns_empty_path_doesnt_navigate()
        {
            //arrange
            var vm = _mocker.CreateInstance<SelectDataSourceViewModel>();
            var sigVm = _mocker.CreateInstance<SingleFileSourceViewModel>();


            _dialogService.Setup(f => f.OpenCsv()).Returns((false, default));

            //act
            vm.FileService.SelectFileCommand.Execute();


            //assert
            _rm.VerifyContentNavigation("SingleFileSourceView", Times.Never());


            sigVm.SelectedFilePath.Should().Be(default);
            sigVm.SelectedFileName.Should().Be(default);
        }



        [Fact]
        public void SelectTSVFiles_when_dialog_returns_valid_path_sets_paths()
        {
            //arrange
            var vm = _mocker.CreateInstance<SelectDataSourceViewModel>();
            var mulVm = _mocker.CreateInstance<MultiFileSourceViewModel>();

            var trainingPath = "C:\\Dir\\training.csv";
            var validationPath = "C:\\Dir\\validation.csv";
            var testPath = "C:\\Dir\\test.csv";

            //disable validation cmd
            _multiFileService.ValidateTrainingFile = _multiFileService.ValidateValidationFile =
                _multiFileService.ValidateTestFile = new DelegateCommand<string>(s => { });

            //act & assert
            //training file
            _dialogService.Setup(f => f.OpenCsv()).Returns((true, trainingPath));
            _multiFileService.SelectTrainingFileCommand.Execute();
            mulVm.TrainingSetFilePath.Should().Be(trainingPath);

            //validation file
            _dialogService.Setup(f => f.OpenCsv()).Returns((true, validationPath));
            _multiFileService.SelectValidationFileCommand.Execute();
            mulVm.ValidationSetFilePath.Should().Be(validationPath);

            //training file
            _dialogService.Setup(f => f.OpenCsv()).Returns((true, testPath));
            _multiFileService.SelectTestFileCommand.Execute();
            mulVm.TestSetFilePath.Should().Be(testPath);
        }


        [Fact]
        public void CreateDataSetCommand_creates_new_session()
        {
            _appState.Sessions.Should().BeEmpty();
            _fileService.Object.CreateDataSetCommand.Execute();

            _appState.Sessions.Should().HaveCount(1);
        }
    }
}