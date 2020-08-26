using System;
using System.ComponentModel;
using System.Reflection;
using Common.Domain;
using Common.Framework;
using FluentAssertions;
using Moq.AutoMock;
using Shell.Interface;
using TestUtils;
using Training.Application.ViewModels;
using Training.Domain;
using Training.Interface;
using Xunit;

namespace Training.Application.Tests
{
    internal static class SessionTestExtensions
    {
        public static void SetupValidAndGate(this Session session)
        {
            session.Network = MLPMocks.AndGateNet;
            session.TrainingData = TrainingDataMocks.AndGateTrainingData();
        }
    }

    public class TrainingParametersTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private ModuleState _moduleState;
        private TrainingParametersViewModel _vm;

        public TrainingParametersTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestVmAccessor();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();

            _vm = new TrainingParametersViewModel(_appState);
        }

        [Fact]
        public void f()
        {
            var session = _appState.CreateSession();
            session.SetupValidAndGate();

            _vm.TrainingParameters.Should().Be(session.TrainingParameters);

            _vm.IsMaxLearningTimeChecked = false;
            _vm.MaxLearningTime = Time.Now.AddMinutes(2);

            var session2 = _appState.CreateSession();
            session2.SetupValidAndGate();

            _appState.ActiveSession = session2;

            _vm.IsMaxLearningTimeChecked.Should().BeTrue();
            _vm.MaxLearningTime.Should().Be(default);


            _appState.ActiveSession = session;
            _vm.IsMaxLearningTimeChecked.Should().BeFalse();
            _vm.MaxLearningTime.Should().BeAtLeast(TimeSpan.FromMinutes(2));

        }

        [Fact]
        public void Properties_are_set_when_changed_before_vm_created()
        {
            var session = _appState.CreateSession();
            session.SetupValidAndGate();
            session.TrainingParameters.MaxLearningTime = TimeSpan.FromMinutes(2);

            var vm = new TrainingParametersViewModel(_appState);
            vm.IsMaxLearningTimeChecked.Should().BeFalse();
            vm.MaxLearningTime.Should().BeAtLeast(TimeSpan.FromMinutes(2));
        }
    }

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
}
