using System.Threading.Tasks;
using Common.Domain;
using FluentAssertions;
using Moq.AutoMock;
using TestUtils;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;
using Xunit;

namespace Training.Application.Tests
{
    public class TrainingControllerTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private TrainingController _ctrl;
        private TrainingViewModel _vm;
        private TrainingService _service;
        private AppState _appState;
        private ModuleState _moduleState;

        public TrainingControllerTests()
        {
            _mocker.UseTestEa();
            _mocker.UseTestRm();
            _mocker.UseTestVmAccessor();

            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();


            _service = _mocker.UseImpl<TrainingService>();

            _ctrl = _mocker.UseImpl<TrainingController>();
            _vm = _mocker.UseVm<TrainingViewModel>();

            _appState.CreateSession();
        }

        private void SetupValidSession()
        {
            _appState.ActiveSession.TrainingData = TrainingDataMocks.AndGateTrainingData();
            _appState.ActiveSession.Network = MLPMocks.AndGateNet;
        }


        [Fact]
        public void ActiveSession_is_valid_commands_can_execute()
        {
            SetupValidSession();

            _moduleState.ActiveSession.IsValid.Should().BeTrue();

            _service.StartTrainingSessionCommand.CanExecute().Should().BeTrue();
            _service.StopTrainingSessionCommand.CanExecute().Should().BeFalse();
            _service.PauseTrainingSessionCommand.CanExecute().Should().BeFalse();

            _service.OpenReportsCommand.CanExecute().Should().BeFalse();

            _service.SelectPanelsClickCommand.CanExecute().Should().BeTrue();
            _service.ResetParametersCommand.CanExecute().Should().BeTrue();

            _service.OpenParametersCommand.CanExecute().Should().BeTrue();
        }

        [Fact]
        public async Task TrainingReport_is_created_after_training_is_paused_or_stopped()
        {
            SetupValidSession();

            _service.StartTrainingSessionCommand.Execute();

            await Task.Delay(50);

            _service.PauseTrainingSessionCommand.Execute();

            await Task.Delay(50);

            _moduleState.ActiveSession.CurrentReport.Should().NotBeNull();

            _service.StartTrainingSessionCommand.Execute();

            await Task.Delay(50);

            _service.StopTrainingSessionCommand.Execute();

            await Task.Delay(50);

            _appState.ActiveSession.TrainingReports.Count.Should().Be(2);
        }


        [Fact]
        public async Task Commands_when_training_is_started_cant_exec_and_can_exec_when_stopped()
        {
            SetupValidSession();

            _service.StartTrainingSessionCommand.Execute();

            await Task.Delay(50);

            _service.StartTrainingSessionCommand.CanExecute().Should().BeFalse();
            _service.StopTrainingSessionCommand.CanExecute().Should().BeTrue();
            _service.PauseTrainingSessionCommand.CanExecute().Should().BeTrue();
            _service.ResetParametersCommand.CanExecute().Should().BeTrue();
            _service.OpenReportsCommand.CanExecute().Should().BeFalse();
            _service.SelectPanelsClickCommand.CanExecute().Should().BeTrue();
            _service.OpenParametersCommand.CanExecute().Should().BeFalse();
            _service.RunTestCommand.CanExecute().Should().BeFalse();
            _service.RunValidationCommand.CanExecute().Should().BeFalse();


            _service.StopTrainingSessionCommand.Execute();
        }
    }
}