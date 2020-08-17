using System.Windows;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Data.Presentation.Services;
using FluentAssertions;
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
        private Mock<MultiFileService> _multiFileService;
        private Mock<ISupervisedDataSetService> _dataSetService;
        private Mock<IFileDialogService> _dialogService;

        private AppState _appState;
        private MultiFileSourceController _ctrl;
        private FileController _fileController;
        private MultiFileSourceViewModel _vm;
        private ITestOutputHelper _testOutput;

        public MultiFileSourceTest(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            _csvValidation = _mocker.UseInMocker<ICsvValidationService>();
            _multiFileService = _mocker.UseInMocker<IMultiFileService, MultiFileService>();
            _dataSetService = _mocker.UseInMocker<ISupervisedDataSetService>();
            _dialogService = _mocker.UseInMocker<IFileDialogService>();
            _appState=_mocker.UseInMocker<AppState>().Object;
            _mocker.UseTestRm();

            _ctrl = _mocker.CreateInstance<MultiFileSourceController>();
            _vm = _mocker.CreateInstance<MultiFileSourceViewModel>();
            _fileController = _mocker.CreateInstance<FileController>();
        }

        [Fact]
        public void Validation_fail_scenario()
        {
            var multiFileService = _multiFileService.Object;

            #region Valid training file

            //setup training file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x", "y"});
            //disable loading
            multiFileService.LoadFiles = new DelegateCommand<(string, string, string)?>(s => { }, s => false);

            multiFileService.ValidateTrainingFile.Execute("training.csv");

            multiFileService.MultiFileValidationResult[0].IsFileValid.Should().BeTrue();

            #endregion


            #region Invalid validation file

            //setup validation file validation
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 1));
            //missing variable 'y'
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] {"x"});
            multiFileService.ValidateValidationFile.Execute("validation.csv");


            var result = multiFileService.MultiFileValidationResult[1];
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

            result = multiFileService.MultiFileValidationResult[2];
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

            result = multiFileService.MultiFileValidationResult[0];

            _testOutput.WriteLine("Training file error: " + result.FileValidationError);
            result.IsFileValid.Should().BeFalse();
            result.FileValidationError.Should().BeEquivalentTo(returnedErr);

            #endregion
        }


        [Fact]
        public void Validation_success_scenario()
        {
            var multiFileService = _multiFileService.Object;


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
            var multiFileService = _multiFileService.Object;

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
            //arrange
            var trainingData = TrainingDataMocks.ValidData1;
            var multiFileService = _multiFileService.Object;

            _dataSetService.Setup(f =>
                f.LoadDefaultSetsFromFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(trainingData);

            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _csvValidation.Setup(f => f.ReadHeaders(It.IsAny<string>())).Returns(new string[] { "x", "y" });

            multiFileService.ValidateTrainingFile.Execute("t.csv");
            multiFileService.ValidateValidationFile.Execute("v.csv");
            multiFileService.ValidateTestFile.Execute("ts.csv");
            multiFileService.LoadFiles.Execute(("t.csv", "v.csv", "ts.csv"));
            multiFileService.ContinueCommand.CanExecute().Should().BeTrue();

            //act
            multiFileService.ContinueCommand.Execute();

            //assert
            _appState.SessionManager.Sessions.Count.Should().Be(1);


            _appState.SessionManager.Sessions.Count.Should().Be(1);
            var session = _appState.SessionManager.ActiveSession;

            session.TrainingData.Should().Be(trainingData);
        }
    }
}