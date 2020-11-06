using System.Threading.Tasks;
using Common.Domain;
using Common.Framework;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Domain.Services;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Commands;
using TestUtils;
using Xunit;

namespace Data.Application.Tests.DataSourceSelection
{
    public class SingleFileSourceTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private Mock<ICsvValidationService> _csvValidation;
        private SingleFileService _singleFileService;
        private Mock<ITrainingDataService> _dataSetService;

        private SingleFileSourceController _ctrl;
        private SingleFileSourceViewModel _vm;
        private AppState _appState;

        public SingleFileSourceTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _mocker.UseTestVmAccessor();
            _appState = _mocker.UseImpl<AppState>();
            _csvValidation = _mocker.UseMock<ICsvValidationService>();
            _dataSetService = _mocker.UseMock<ITrainingDataService>();
            _ctrl = _mocker.UseImpl<ITransientController<SingleFileService>, SingleFileSourceController>();
            _singleFileService = _mocker.UseImpl<ISingleFileService, SingleFileService>();

            _vm = _mocker.UseVm<SingleFileSourceViewModel>();
        }

        [Fact]
        public async void Validation_success_scenario()
        {
            //arrange
            var singleFileService = _singleFileService;

            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));

            //act & assert
            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeFalse();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();

            //stop load exec
            var prev = singleFileService.LoadCommand;
            singleFileService.LoadCommand = new DelegateCommand<string>(_ => { }, _ => false);

            singleFileService.ValidateCommand.Execute("file.csv");
            //wait for async call completion
            await Task.Delay(500);

            singleFileService.LoadCommand = prev;

            //file is valid
            singleFileService.FileValidationResult.IsFileValid.Should().BeTrue();
            //can run validate cmd
            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            //can run load cmd
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeTrue();
            //can return
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();
            //cannot continue
            singleFileService.ContinueCommand.CanExecute().Should().BeFalse();
        }

        [Fact]
        public async void Validation_fail_scenario()
        {
            //arrange
            var singleFileService = _singleFileService;
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((false, "error", 2, 2));

            //act & assert
            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeFalse();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();

            singleFileService.ValidateCommand.Execute("file.csv");
            //wait for async call completion
            await Task.Delay(500);

            singleFileService.FileValidationResult.IsFileValid.Should().BeFalse();

            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeFalse();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();
        }


        [Fact]
        public async Task Continue_command_creates_new_session_if_0_sessions()
        {
            var file = "C:\\file.csv";
            //arrange
            var trainingData = TrainingDataMocks.ValidData1;
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _dataSetService.Setup(f => f.LoadDefaultTrainingData(It.IsAny<string>(), null,null,null)).Returns(trainingData);
            var singleFileService = _singleFileService;


            //act & assert
            singleFileService.ContinueCommand.CanExecute().Should().BeFalse();

            //set and trigger validation cmd
            _vm.SelectedFilePath = file;

            //wait for async call completion
            await Task.Delay(2_000);

            //variables are set
            _vm.Variables.Length.Should().BeGreaterThan(0);
            singleFileService.ContinueCommand.CanExecute().Should().BeTrue();

            singleFileService.ContinueCommand.Execute();

            _appState.Sessions.Count.Should().Be(1);
            var session = _appState.ActiveSession;

            session.TrainingData.Should().Be(trainingData);
            session.SingleDataFile.Should().Be(file);
        }

        [Fact]
        public async void Continue_command_updates_data_of_active_session_with_null_data()
        {
            var session = _appState.CreateSession();
            
            await Continue_command_creates_new_session_if_0_sessions();
        }
    }
}