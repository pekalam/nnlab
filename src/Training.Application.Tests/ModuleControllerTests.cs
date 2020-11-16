using System.Threading.Tasks;
using Common.Domain;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Regions;
using Shell.Interface;
using TestUtils;
using Training.Domain;
using Training.Interface;
using Xunit;
#pragma warning disable 4014

namespace Training.Application.Tests
{
    internal static class SessionTestExtensions
    {
        public static void SetupValidAndGate(this Session session)
        {
            session.TrainingData = TrainingDataMocks.AndGateTrainingData();
            session.Network = MLPMocks.AndGateNet;
        }
    }

    public class ModuleControllerTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private ModuleController _ctrl;
        private TestEa _ea;
        private ModuleState _moduleState;
        private Mock<IRegionManager> _rm;

        public ModuleControllerTests()
        {
            _ea = _mocker.UseTestEa();
            (_rm,_) = _mocker.UseTestRm();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState =_mocker.UseImpl<ModuleState>();

            _ctrl = _mocker.CreateInstance<ModuleController>();
        }

        [Fact]
        public void TrainingSession_is_created_and_changed_with_active_session()
        {
            var session = _appState.CreateSession();

            //should be created but invalid
            _moduleState.ActiveSession.Should().NotBeNull();
            _moduleState.ActiveSession.IsValid.Should().BeFalse();


            session.TrainingData = TrainingDataMocks.ValidData1;

            //should be created but invalid
            _moduleState.ActiveSession.Should().NotBeNull();
            _moduleState.ActiveSession.IsValid.Should().BeFalse();

            session.Network = MLPMocks.ValidNet1;

            //should be created and valid
            _moduleState.ActiveSession.Should().NotBeNull();
            _moduleState.ActiveSession.IsValid.Should().BeTrue();


            var first = _moduleState.ActiveSession;

            _appState.ActiveSession = _appState.CreateSession();

            //should not be the same as previous
            _moduleState.ActiveSession.Should().NotBeNull();
            first.Should().NotBe(_moduleState.ActiveSession);

            _appState.ActiveSession = session;

            first.Should().Be(_moduleState.ActiveSession);
        }

        [Fact]
        public void Enable_events_are_sent_when_active_training_session_is_valid()
        {
            _ctrl.Run();
            _ea.VerifyTimesCalled<EnableNavMenuItem>(0);

            //invalid session is created
            var session = _appState.CreateSession();

            //sends disabled
            _ea.VerifyTimesCalled<DisableNavMenuItem>(1);
            
            //updated to valid
            session.Network = MLPMocks.ValidNet1;
            session.TrainingData = TrainingDataMocks.ValidData1;

            //enabled
            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);

            //created and switched to new
            var session2 = _appState.CreateSession();
            _appState.ActiveSession = session2;

            _ea.VerifyTimesCalled<DisableNavMenuItem>(2);


            session2.Network = MLPMocks.ValidNet1;
            session2.TrainingData = TrainingDataMocks.ValidData1;

            _ea.VerifyTimesCalled<EnableNavMenuItem>(2);

            //invalid created
            var session3 = _appState.CreateSession();
            _appState.ActiveSession = session3;

            _ea.VerifyTimesCalled<DisableNavMenuItem>(3);

            //switched to first - valid
            _appState.ActiveSession = session;

            _ea.VerifyTimesCalled<EnableNavMenuItem>(3);

            //switched to invalid
            _appState.ActiveSession = session3;
            _ea.VerifyTimesCalled<DisableNavMenuItem>(4);

            //updated to valid
            session3.Network = MLPMocks.ValidNet1;
            session3.TrainingData = TrainingDataMocks.ValidData1;
            //should be called 4 times because event handlers should be set once
            _ea.VerifyTimesCalled<EnableNavMenuItem>(4);
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

            await Task.Delay(100);

            await _moduleState.ActiveSession.Pause();
            _ea.VerifyTimesCalled<TrainingSessionPaused>(1);

            await _moduleState.ActiveSession.Stop();
            _ea.VerifyTimesCalled<TrainingSessionStopped>(1);
        }


        [Fact]
        public void When_new_session_is_active_without_net_and_data_sends_disabled_and_module_state_change_events()
        {
            int called = 0;
            TrainingSession? prev = null;
            TrainingSession next = null;
            _moduleState.ActiveSessionChanged += (sender, tuple) =>
            {
                (prev, next) = tuple;
                called++;
            };

            _ctrl.Run();

            var session = _appState.CreateSession();
            _ea.VerifyTimesCalled<DisableNavMenuItem>(1);
            called.Should().Be(1);
            prev.Should().BeNull();
            next.Should().NotBeNull();

            _appState.ActiveSession = _appState.CreateSession();
            _ea.VerifyTimesCalled<DisableNavMenuItem>(2);
            called.Should().Be(2);
            prev.Should().NotBeNull();
            next.Should().NotBeNull();
        }
    }
}
