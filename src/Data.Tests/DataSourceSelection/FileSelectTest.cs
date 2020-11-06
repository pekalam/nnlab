using System.Collections.Generic;
using Common.Domain;
using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Interfaces;
using Data.Application.Services;
using Data.Application.ViewModels;
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
        private ISingleFileService _singleFileService;
        private IMultiFileService _multiFileService;
        private Mock<IFileDialogService> _dialogService;
        private Mock<IRegionManager> _rm;

        private FileController _ctrl;

        public FileSelectTest()
        {
            (_rm, _) = _mocker.UseTestRm();
            _mocker.UseTestEa();
            _appState = _mocker.UseImpl<AppState>();
            _dialogService = _mocker.UseMock<IFileDialogService>();
            _singleFileService = _mocker.UseImpl<ISingleFileService, SingleFileSourceController>();
            _multiFileService = _mocker.UseImpl<IMultiFileService, MultiFileSourceController>();


            _ctrl= _mocker.UseImpl<IFileService,FileController>();
        }

        [Fact]
        public void SelectFileCommand_when_dialog_returns_path_navigates_to_next_view()
        {
            //arrange
            var vm = _mocker.UseVm<SelectDataSourceViewModel>(); //current vm
            var sigVm = _mocker.UseVm<SingleFileSourceViewModel>(); //next view vm

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
            var vm = _mocker.UseVm<SelectDataSourceViewModel>();
            var sigVm = _mocker.UseVm<SingleFileSourceViewModel>();


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
            var vm = _mocker.UseVm<SelectDataSourceViewModel>();
            var mulVm = _mocker.UseVm<MultiFileSourceViewModel>();

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
        public void CreateDataSetCommand_creates_new_session_if_0_sessions()
        {
            _ctrl.CreateDataSetCommand.Execute();

            _appState.Sessions.Should().HaveCount(1);
        }

        [Fact]
        public void CreateDataSetCommand_does_not_create_new_session_if_there_is_active()
        {
            _appState.CreateSession();
            CreateDataSetCommand_creates_new_session_if_0_sessions();
        }
    }
}