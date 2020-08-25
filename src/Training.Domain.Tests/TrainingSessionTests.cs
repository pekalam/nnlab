using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Domain;
using FluentAssertions;
using NNLib;
using TestUtils;
using Xunit;

namespace Training.Domain.Tests
{
    public class TrainingSessionTests
    {
        MLPNetwork net = new MLPNetwork(new PerceptronLayer(2, 2, new LinearActivationFunction()),
            new PerceptronLayer(2, 1, new SigmoidActivationFunction()));
        TrainingData data = TrainingDataMocks.AndGateTrainingData();
        private int epochs;
        private double lastErr;

        private AppState _appState;
        private ModuleState _moduleState;

        public TrainingSessionTests()
        {
            _appState = new AppState();
            _moduleState = new ModuleState(_appState);
            var session=_appState.CreateSession();

            session.TrainingData = data;
            session.Network = net;
        }
    
        private void SetupEpochEndHandler(TrainingSession session)
        {
            session.EpochEnd += (sender, args) =>
            {
                epochs++;
                args.Epoch.Should().Be(epochs);
                args.Error.Should().BeGreaterThan(0);
                sender.Should().Be(session);
                lastErr = args.Error;
            };
        }
    
        [Fact]
        public async Task Start_when_max_epoch_params_returns_valid_report()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 10
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;
            SetupEpochEndHandler(session);
    
            var report = await session.Start();
            session.StartTime.Should().BeWithin(TimeSpan.FromMinutes(1));

            epochs.Should().Be(10);
    
            report.SessionEndType.Should().Be(SessionEndType.MaxEpoch);
            report.TotalEpochs.Should().Be(10);
            report.Error.Should().Be(lastErr);
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
            _appState.ActiveSession!.TrainingReports.First().Should().Be(report);
        }
    
        [Fact]
        public async Task Start_when_target_reached_params_returns_valid_report()
        {
            var param = new TrainingParameters()
            {
                TargetError = 10000
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

            SetupEpochEndHandler(session);
    
            var report = await session.Start();
    
            epochs.Should().BeGreaterThan(0);
    
            report.SessionEndType.Should().Be(SessionEndType.TargetReached);
            report.TotalEpochs.Should().BeGreaterThan(0);
            report.Error.Should().Be(lastErr);
    
            lastErr.Should().BeLessOrEqualTo(param.TargetError);
        }
    
        [Fact]
        public async void Stop_when_called_stops_session_and_resets()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

            SetupEpochEndHandler(session);
    
            var t = session.Start();
    
            try
            {
                await session.Stop();
            }
            catch (TrainingCanceledException)
            {
            }
    
            var report = t.GetAwaiter().GetResult();
            report.SessionEndType.Should().Be(SessionEndType.Stopped);
            session.Started.Should().BeFalse();
            session.Stopped.Should().BeTrue();
            session.Trainer.Epochs.Should().Be(0);
            session.Trainer.Iterations.Should().Be(0);
            session.CurrentReport.Should().Be(report);

            
        }
    
        [Fact]
        public async void Stop_when_called_stops_session_and_next_start_call_throws()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

            SetupEpochEndHandler(session);
            var t = session.Start();
            try
            {
                await session.Stop();
            }
            catch (TrainingCanceledException)
            {
            }
    
            await Assert.ThrowsAsync<AggregateException>(() => session.Start());
        }
    
    
        [Fact]
        public async void Pause_does_not_reset_epochs()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 100000000
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

            SetupEpochEndHandler(session);
    
            var t = session.Start();
    
            await Task.Delay(1000);
    
            try
            {
                await session.Pause();
            }
            catch (TrainingCanceledException)
            {
            }

            session.Paused.Should().BeTrue();
            session.Stopped.Should().BeFalse();
            session.Started.Should().BeFalse();
            session.Trainer.Epochs.Should().NotBe(0);
            session.CurrentReport.SessionEndType.Should().Be(SessionEndType.Paused);
        }
    
    
        [Fact]
        public async void Stop_when_called_after_pause_returns()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

            SetupEpochEndHandler(session);
            var t = session.Start();
            try
            {
                await session.Pause();
            }
            catch (TrainingCanceledException)
            {
            }
    
            await session.Stop();

            session.Paused.Should().BeFalse();
            session.Stopped.Should().BeTrue();
            session.Started.Should().BeFalse();
            session.CurrentReport.SessionEndType.Should().Be(SessionEndType.Stopped);
        }
    
    
        [Fact]
        public async void Stop_when_not_started_returns_report()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

    
    
            await session.Stop();

            session.Paused.Should().BeFalse();
            session.Stopped.Should().BeTrue();
            session.Started.Should().BeFalse();
            session.CurrentReport.SessionEndType.Should().Be(SessionEndType.Stopped);
        }
    
    
        [Fact]
        public async void When_max_epochs_reached_returns_valid_report()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 1
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

    
            var report = await session.Start();
    
            report.SessionEndType.Should().Be(SessionEndType.MaxEpoch);
            session.Paused.Should().BeFalse();
            session.Stopped.Should().BeTrue();
            session.Started.Should().BeFalse();
        }

        [Fact]
        public async void When_max_learning_time_reached_returns_valid_report()
        {
            var param = new TrainingParameters()
            {
                MaxEpochs = 1_000_000,
                MaxLearningTime = TimeSpan.FromSeconds(1)
            };

            _appState.ActiveSession.TrainingParameters = param;
            var session = _moduleState.ActiveSession;

            var task = session.Start();
    
            await Task.Delay(3000);
    
            var report = await task;
    
            report.SessionEndType.Should().Be(SessionEndType.Timeout);
            session.Stopped.Should().BeTrue();
            session.Paused.Should().BeFalse();
            session.Started.Should().BeFalse();
        }
    }
}