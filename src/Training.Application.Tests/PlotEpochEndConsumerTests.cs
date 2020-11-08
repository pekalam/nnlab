using Common.Domain;
using FluentAssertions;
using Moq.AutoMock;
using System.Threading.Tasks;
using TestUtils;
using Training.Application.Plots;
using Training.Domain;
using Xunit;

namespace Training.Application.Tests
{
    internal class TestTrainingSession : TrainingSession
    {
        private bool _testStarted;
        private bool _testPaused;
        private bool _testStopped;

        public TestTrainingSession(AppState appState) : base(appState)
        {
        }

        public override bool Started => TestStarted;
        public override bool Stopped => TestStopped;
        public override bool Paused => TestPaused;

        public bool TestStarted
        {
            get => _testStarted;
            set
            {
                _testStarted = value;
                RaisePropertyChanged(nameof(Started));
            }
        }

        public bool TestPaused
        {
            get => _testPaused;
            set
            {
                _testPaused = value;
                RaisePropertyChanged(nameof(Paused));
            }
        }

        public bool TestStopped
        {
            get => _testStopped;
            set
            {
                _testStopped = value;
                RaisePropertyChanged(nameof(Stopped));
            }
        }
    }

    public class PlotEpochEndConsumerTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private ModuleState _moduleState;
        private AppState _appState;
        private PlotEpochEndConsumer _epochEndConsumer;

        private int _onTrainingStartingCalled;
        private int _onTrainingPausedCalled;
        private int _onTrainingStoppedCalled;
        private int _callbackCalled;

        public PlotEpochEndConsumerTests()
        {
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();

            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState,
                (args, session) => { _callbackCalled++; },
                trainingSession =>
                {
                    _onTrainingStartingCalled++;
                },
                trainingSession => { _onTrainingStoppedCalled++; }, session => { _onTrainingPausedCalled++; });
        }

        [Fact]
        public async void f()
        {
            //sim online sub
            await Task.Delay(1000);

            _onTrainingPausedCalled.Should().Be(0);
            _onTrainingStartingCalled.Should().Be(0);
            _onTrainingStoppedCalled.Should().Be(0);
            _callbackCalled.Should().Be(0);
        }
    }
}
