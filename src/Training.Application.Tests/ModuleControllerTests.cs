using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using Common.Domain;
using Common.Framework;
using FluentAssertions;
using Moq.AutoMock;
using Shell.Interface;
using TestUtils;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;
using Training.Interface;
using Xunit;

namespace Training.Application.Tests
{
    public class ModuleControllerTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private ModuleController _ctrl;
        private TestEa _ea;
        private ModuleState _moduleState;

        public ModuleControllerTests()
        {
            _ea = _mocker.UseTestEa();
            _mocker.UseTestRm();
            _mocker.UseTestVmAccessor();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState =_mocker.UseImpl<ModuleState>();

            _ctrl = _mocker.CreateInstance<ModuleController>();
        }

        [Fact]
        public void TrainingSession_is_created__events_are_sent()
        {
            _ctrl.Run();
            _ea.VerifyTimesCalled<EnableNavMenuItem>(0);
            var session = _appState.CreateSession();
            session.Network = MLPMocks.ValidNet1;
            session.TrainingData = TrainingDataMocks.ValidData1;

            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);
        }


        [Fact]
        public async void TrainingSession_is_started_paused_and_stopped__events_are_sent()
        {
            _ctrl.Run();
            var session = _appState.CreateSession();
            session.Network = MLPMocks.AndGateNet;
            session.TrainingData = TrainingDataMocks.AndGateTrainingData();

            _moduleState.ActiveSession.Start();
            _ea.VerifyTimesCalled<TrainingSessionStarted>(1);

            await _moduleState.ActiveSession.Pause();
            _ea.VerifyTimesCalled<TrainingSessionPaused>(1);

            await _moduleState.ActiveSession.Stop();
            _ea.VerifyTimesCalled<TrainingSessionStopped>(1);
        }
    }



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

        [Fact]
        public void ActiveSession_is_valid_commands_can_execute()
        {
            _appState.ActiveSession.TrainingData = TrainingDataMocks.ValidData1;
            _appState.ActiveSession.Network = MLPMocks.ValidNet1;

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
        public async void TrainingReport_is_created_after_training_is_stopped()
        {
            _appState.ActiveSession.TrainingData = TrainingDataMocks.AndGateTrainingData();
            _appState.ActiveSession.Network = MLPMocks.AndGateNet;


            _service.StartTrainingSessionCommand.Execute();

            await Task.Delay(1000);

            _service.StopTrainingSessionCommand.Execute();

            await Task.Delay(500);

            _moduleState.ActiveSession.CurrentReport.Should().NotBeNull();
        }
    }
}
