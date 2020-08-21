using System.Windows;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Application.ViewModels.DataSourceSelection;
using Data.Domain.Services;
using Data.Presentation.Services;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Domain;
using Moq;
using Moq.AutoMock;
using Prism.Commands;
using TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Data.Tests.Application.DataSourceSelection
{
    public class MultiFileSourceTest
    {
        private AutoMocker _mocker = new AutoMocker();
        private Mock<ICsvValidationService> _csvValidation;
        private MultiFileService _multiFileService;
        private Mock<ISupervisedDataSetService> _dataSetService;
        private Mock<IFileDialogService> _dialogService;
        private Mock<FileService> _fileService;

        private AppState _appState;
        private MultiFileSourceController _ctrl;
        private FileController _fileController;
        private MultiFileSourceViewModel _vm;
        private ITestOutputHelper _testOutput;

        public MultiFileSourceTest(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _mocker.UseTestRm();
            _dataSetService = _mocker.UseMock<ISupervisedDataSetService>();
            _dialogService = _mocker.UseMock<IFileDialogService>();
            _appState = _mocker.UseMock<AppState>().Object;
            _csvValidation = _mocker.UseMock<ICsvValidationService>();
            _ctrl = _mocker.UseImpl<ITransientControllerBase<MultiFileService>, MultiFileSourceController>();
            _multiFileService = _mocker.UseImpl<IMultiFileService, MultiFileService>();
            _fileService = _mocker.UseMock<IFileService, FileService>();

            _vm = _mocker.CreateInstance<MultiFileSourceViewModel>();
            _fileController = _mocker.CreateInstance<FileController>();
        }

        [Fact]
        public void Validation_fail_scenario()
        {
            var multiFileService = _multiFileService;

            #region Valid training file

            //setup training file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x", "y"});
            //disable loading
            multiFileService.LoadFiles = new DelegateCommand<(string, string, string)?>(s => { }, s => false);

            multiFileService.ValidateTrainingFile.Execute("training.csv");

            _vm.MultiFileValidationResult[0].IsFileValid.Should().BeTrue();

            #endregion


            #region Invalid validation file

            //setup validation file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 1));
            //missing variable 'y'
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x"});
            multiFileService.ValidateValidationFile.Execute("validation.csv");


            var result = _vm.MultiFileValidationResult[1];
            result.IsFileValid.Should().BeFalse();
            result.FileValidationError.Should().NotBeNullOrEmpty();

            _testOutput.WriteLine("Validation file error: " + result.FileValidationError);

            #endregion


            #region Invalid test file

            //setup test file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            //missing invalid variable name
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"xX", "y"});
            multiFileService.ValidateTestFile.Execute("test.csv");

            _testOutput.WriteLine("Test file error: " + result.FileValidationError);

            result = _vm.MultiFileValidationResult[2];
            result.IsFileValid.Should().BeFalse();
            result.FileValidationError.Should().NotBeNullOrEmpty();

            #endregion


            #region Set validation and test files valid then set invalid training file

            //set valid
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            multiFileService.ValidateValidationFile.Execute("validation.csv");
            multiFileService.ValidateTestFile.Execute("test.csv");


            //setup training file validation
            //invalid file
            string returnedErr = "error";
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((false, returnedErr, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x", "y"});

            //reset training file validation
            result.IsFileValid = null;
            result.FileValidationError = null;

            multiFileService.ValidateTrainingFile.Execute("training.csv");

            result = _vm.MultiFileValidationResult[0];

            _testOutput.WriteLine("Training file error: " + result.FileValidationError);
            result.IsFileValid.Should().BeFalse();
            result.FileValidationError.Should().BeEquivalentTo(returnedErr);

            #endregion
        }


        [Fact]
        public void Validation_success_scenario()
        {
            var multiFileService = _multiFileService;


            //setup training file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            //disable loading
            multiFileService.LoadFiles = new DelegateCommand<(string,string,string)?>(s => { }, s => false);

            multiFileService.ValidateTrainingFile.Execute("training.csv");

            multiFileService.TrainingValidationResult.IsFileValid.Should().BeTrue();



            //test file error
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            //invalid var name
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "xy", "y" });

            multiFileService.ValidateTestFile.Execute("test.csv");

            multiFileService.TestValidationResult.IsFileValid.Should().BeFalse();
            multiFileService.TrainingValidationResult.IsFileValid.Should().BeFalse();


            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            multiFileService.ValidateTestFile.Execute("test.csv");


            multiFileService.TestValidationResult.IsFileValid.Should().BeTrue();
            multiFileService.TrainingValidationResult.IsFileValid.Should().BeTrue();

            multiFileService.ValidateValidationFile.Execute("test.csv");


            multiFileService.TestValidationResult.IsFileValid.Should().BeTrue();
            multiFileService.TrainingValidationResult.IsFileValid.Should().BeTrue();
            multiFileService.ValidationValidationResult.IsFileValid.Should().BeTrue();

        }

        [Fact]
        public void Select_file_command_resets_validation_result()
        {
            var multiFileService = _multiFileService;

            _dialogService.Setup(f => f.OpenCsv(It.IsAny<Window>())).Returns((true, "x.csv"));
            //setup training file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((false, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            //disable loading
            multiFileService.LoadFiles = new DelegateCommand<(string, string, string)?>(s => { }, s => false);

            multiFileService.ValidateTrainingFile.Execute("training.csv");

            var previous = multiFileService.TrainingValidationResult;

            multiFileService.SelectTrainingFileCommand.Execute();


            multiFileService.TrainingValidationResult.Should().NotBe(previous);
        }


        [Fact]
        public void Continue_command_sets_app_state()
        {
            var trainingFile = "C:\\t.csv";
            var testFile = "C:\\t.csv";
            var validationFile = "C:\\t.csv";

            //arrange
            var trainingData = TrainingDataMocks.ValidData1;
            var multiFileService = _multiFileService;

            _dataSetService.Setup(f =>
                f.LoadDefaultSetsFromFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(trainingData);

            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });

            //trigger validation commands
            _vm.TrainingSetFilePath = trainingFile;
            _vm.ValidationSetFilePath = validationFile;
            _vm.TestSetFilePath = testFile;

            multiFileService.LoadFiles.Execute((trainingFile, validationFile, testFile));
            multiFileService.ContinueCommand.CanExecute().Should().BeTrue();

            //act
            multiFileService.ContinueCommand.Execute();

            //assert
            _appState.SessionManager.Sessions.Count.Should().Be(1);


            _appState.SessionManager.Sessions.Count.Should().Be(1);
            var session = _appState.SessionManager.ActiveSession;

            session.TrainingData.Should().Be(trainingData);
            session.SingleDataFile.Should().BeNull();
            session.TrainingDataFile.Should().Be(trainingFile);
            session.ValidationDataFile.Should().Be(validationFile);
            session.TestDataFile.Should().Be(testFile);

        }
    }
}