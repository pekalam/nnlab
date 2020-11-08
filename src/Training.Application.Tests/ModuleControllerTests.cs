using Common.Domain;
using Moq;
using Moq.AutoMock;
using Prism.Regions;
using Shell.Interface;
using TestUtils;
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

            await _moduleState.ActiveSession.Pause();
            _ea.VerifyTimesCalled<TrainingSessionPaused>(1);

            await _moduleState.ActiveSession.Stop();
            _ea.VerifyTimesCalled<TrainingSessionStopped>(1);
        }


        [Fact]
        public void When_new_session_is_active_without_net_and_data_sends_disabled()
        {
            _ctrl.Run();

            var session = _appState.CreateSession();
            _ea.VerifyTimesCalled<DisableNavMenuItem>(1);

            _appState.ActiveSession = _appState.CreateSession();
            _ea.VerifyTimesCalled<DisableNavMenuItem>(2);
        }
    }
}
