using Common.Domain;
using Data.Application.Controllers;
using Data.Application.Interfaces;

using Data.Application.ViewModels;
using Data.Domain.Services;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Commands;
using TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace Data.Application.Tests.DataSourceSelection
{
    public class MultiFileSourceTest
    {
        private AutoMocker _mocker = new AutoMocker();
        private Mock<ICsvValidationService> _csvValidation;
        private MultiFileSourceController _multiFileController;
        private Mock<ITrainingDataService> _dataSetService;
        private Mock<IFileDialogService> _dialogService;

        private AppState _appState;
        private MultiFileSourceController _ctrl;
        private MultiFileSourceViewModel _vm;
        private ITestOutputHelper _testOutput;

        public MultiFileSourceTest(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _appState = _mocker.UseImpl<AppState>();
            _dataSetService = _mocker.UseMock<ITrainingDataService>();
            _dialogService = _mocker.UseMock<IFileDialogService>();
            _csvValidation = _mocker.UseMock<ICsvValidationService>();
            _ctrl = _mocker.UseImpl<IMultiFileSourceController, MultiFileSourceController>();
            _multiFileController = _mocker.UseImpl<IMultiFileSourceController, MultiFileSourceController>();

            _vm = _mocker.UseVm<MultiFileSourceViewModel>();
        }

        [Fact]
        public void Validation_fail_scenario()
        {
            var ctrl = _multiFileController;

            #region Valid training file

            //setup training file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x", "y"});
            //disable loading
            ctrl.LoadFiles = new DelegateCommand<(string, string, string)?>(s => { }, s => false);

            ctrl.ValidateTrainingFile.Execute("training.csv");

            _vm.MultiFileValidationResult[0].IsFileValid.Should().BeTrue();

            #endregion


            #region Invalid validation file

            //setup validation file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 1));
            //missing variable 'y'
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x"});
            ctrl.ValidateValidationFile.Execute("validation.csv");


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
            ctrl.ValidateTestFile.Execute("test.csv");

            _testOutput.WriteLine("Test file error: " + result.FileValidationError);

            result = _vm.MultiFileValidationResult[2];
            result.IsFileValid.Should().BeFalse();
            result.FileValidationError.Should().NotBeNullOrEmpty();

            #endregion


            #region Set validation and test files valid then set invalid training file

            //set valid
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            ctrl.ValidateValidationFile.Execute("validation.csv");
            ctrl.ValidateTestFile.Execute("test.csv");


            //setup training file validation
            //invalid file
            string returnedErr = "error";
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((false, returnedErr, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x", "y"});

            //reset training file validation
            result.IsFileValid = null;
            result.FileValidationError = null;

            ctrl.ValidateTrainingFile.Execute("training.csv");

            result = _vm.MultiFileValidationResult[0];

            _testOutput.WriteLine("Training file error: " + result.FileValidationError);
            result.IsFileValid.Should().BeFalse();
            result.FileValidationError.Should().BeEquivalentTo(returnedErr);

            #endregion
        }


        [Fact]
        public void Validation_success_scenario()
        {
            var ctrl = _multiFileController;


            //setup training file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            //disable loading
            var previousLoadCmd = ctrl.LoadFiles;
            ctrl.LoadFiles = new DelegateCommand<(string,string,string)?>(s => { }, s => false);

            ctrl.ValidateTrainingFile.Execute("training.csv");

            _vm.TrainingValidationResult.IsFileValid.Should().BeTrue();



            //test file error
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            //invalid var name
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "xy", "y" });

            ctrl.ValidateTestFile.Execute("test.csv");

            _vm.TestValidationResult.IsFileValid.Should().BeFalse();
            _vm.TrainingValidationResult.IsFileValid.Should().BeFalse();


            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            ctrl.ValidateTestFile.Execute("test.csv");


            _vm.TestValidationResult.IsFileValid.Should().BeTrue();
            _vm.TrainingValidationResult.IsFileValid.Should().BeTrue();

            ctrl.ValidateValidationFile.Execute("test.csv");


            _vm.TestValidationResult.IsFileValid.Should().BeTrue();
            _vm.TrainingValidationResult.IsFileValid.Should().BeTrue();
            _vm.ValidationValidationResult.IsFileValid.Should().BeTrue();

            //can load file
            ctrl.LoadFiles = previousLoadCmd;
            ctrl.LoadFiles.CanExecute((_vm.TrainingSetFilePath, _vm.ValidationSetFilePath, _vm.TestSetFilePath)).Should().BeTrue();
        }

        [Fact]
        public void Select_file_command_resets_validation_result()
        {
            var multiFileService = _multiFileController;

            _dialogService.Setup(f => f.OpenCsv()).Returns((true, "x.csv"));
            //setup training file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((false, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });
            //disable loading
            multiFileService.LoadFiles = new DelegateCommand<(string, string, string)?>(s => { }, s => false);

            multiFileService.ValidateTrainingFile.Execute("training.csv");

            var previous = _vm.TrainingValidationResult;

            multiFileService.SelectTrainingFileCommand.Execute();


            _vm.TrainingValidationResult.Should().NotBe(previous);
        }


        [Theory]
        [InlineData("C:\\t.csv", "C:\\v.csv", "C:\\ts.csv")]
        [InlineData("C:\\t.csv", null, "C:\\ts.csv")]
        [InlineData("C:\\t.csv", "C:\\v.csv", null)]
        public void Continue_command_creates_new_session_if_0_sessions(string trainingFile, string validationFile, string testFile)
        {
            //arrange
            var trainingData = TrainingDataMocks.ValidData1;
            var ctrl = _multiFileController;

            _dataSetService.Setup(f =>
                f.LoadDefaultTrainingDataFromFiles(trainingFile, It.Is<string>(s => s == validationFile || s == null),
                    It.Is<string>(s => s == testFile || s == null)))
                .Returns(trainingData);

            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });

            //trigger validation commands
            _vm.TrainingSetFilePath = trainingFile;
            _vm.ValidationSetFilePath = validationFile;
            _vm.TestSetFilePath = testFile;

            ctrl.LoadFiles.CanExecute((trainingFile, validationFile, testFile)).Should().BeTrue();
            //should be executed by load files
            _dataSetService.Verify(service => service.LoadDefaultTrainingDataFromFiles(trainingFile, validationFile, testFile), Times.Once());
            ctrl.ContinueCommand.CanExecute().Should().BeTrue();

            //act
            ctrl.ContinueCommand.Execute();

            //assert
            _appState.Sessions.Count.Should().Be(1);


            _appState.Sessions.Count.Should().Be(1);
            var session = _appState.ActiveSession;

            session.TrainingData.Should().Be(trainingData);
            session.SingleDataFile.Should().BeNull();
            session.TrainingDataFile.Should().Be(trainingFile);
            session.ValidationDataFile.Should().Be(validationFile);
            session.TestDataFile.Should().Be(testFile);

        }

        [Fact]
        public void Continue_command_updates_data_of_active_session_with_null_data()
        {
            var session = _appState.CreateSession();

            Continue_command_creates_new_session_if_0_sessions("C:\\t.csv", "C:\\v.csv", "C:\\ts.csv");
        }
    }
}