using Common.Domain;
using FluentAssertions;
using NNLib.ActivationFunction;
using NNLib.Exceptions;
using NNLib.MLP;
using NNLib.Training.GradientDescent;
using NNLib.Training.LevenbergMarquardt;
using System;
using System.Linq;
using System.Threading.Tasks;
using NNLib;
using NNLib.Data;
using TestUtils;
using Xunit;

namespace Training.Domain.Tests
{
    public class TrainingSessionTests
    {
        private int epochs;
        private double lastErr;

        private AppState _appState;
        private TrainingSession _session;

        public TrainingSessionTests()
        {
            _appState = new AppState();
            var session=_appState.CreateSession();
            _session = new TrainingSession(_appState);

            session.TrainingData = TrainingDataMocks.AndGateTrainingData();
            session.Network = MLPMocks.AndGateNet;
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
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 10
            };

            _appState.ActiveSession.TrainingParameters = param;
            SetupEpochEndHandler(_session);
    
            var report = await _session.Start();
            _session.StartTime.Should().BeWithin(TimeSpan.FromMinutes(1));

            epochs.Should().Be(10);
    
            report.SessionEndType.Should().Be(SessionEndType.MaxEpoch);
            report.TotalEpochs.Should().Be(10);
            report.Error.Should().Be(lastErr);
            report.EpochEndEventArgs.Should().HaveCount(10);

            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
            _appState.ActiveSession!.TrainingReports.First().Should().Be(report);
        }
    
        [Fact]
        public async Task Start_when_target_reached_params_returns_valid_report()
        {
            var param = new TrainingParameters(false)
            {
                TargetError = 10000000,
            };

            _appState.ActiveSession.TrainingParameters = param;

            SetupEpochEndHandler(_session);
    
            var report = await _session.Start();
    
            epochs.Should().BeGreaterThan(0);
    
            report.SessionEndType.Should().Be(SessionEndType.TargetReached);
            report.TotalEpochs.Should().BeGreaterThan(0);
            report.Error.Should().Be(lastErr);
            report.EpochEndEventArgs.Should().HaveCount(report.TotalEpochs);

            lastErr.Should().BeLessOrEqualTo(param.TargetError);
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
        }

        [Fact]
        public async void Stop_when_called_stops_session_and_creates_report()
        {
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;

            SetupEpochEndHandler(_session);
    
            var t = _session.Start();
    
            try
            {
                await _session.Stop();
            }
            catch (TrainingCanceledException)
            {
            }
    
            var report = t.GetAwaiter().GetResult();
            report.SessionEndType.Should().Be(SessionEndType.Stopped);
            _session.Started.Should().BeFalse();
            _session.Stopped.Should().BeTrue();
            _session.CurrentReport.Should().Be(report);
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
        }

        [Fact]
        public async void Stop_when_called_stops_session_and_next_start_call_throws()
        {
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;

            SetupEpochEndHandler(_session);
            var t = _session.Start();
            try
            {
                await _session.Stop();
            }
            catch (TrainingCanceledException)
            {
            }
    
            await Assert.ThrowsAsync<Exception>(() => _session.Start());
        }
    
    
        [Fact]
        public async void Pause_does_not_reset_epochs()
        {
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 100000000
            };

            _appState.ActiveSession.TrainingParameters = param;

            SetupEpochEndHandler(_session);
    
            var t = _session.Start();
    
            await Task.Delay(1000);
    
            try
            {
                await _session.Pause();
            }
            catch (TrainingCanceledException)
            {
            }

            _session.Paused.Should().BeTrue();
            _session.Stopped.Should().BeFalse();
            _session.Started.Should().BeFalse();
            _session.Trainer.Epochs.Should().NotBe(0);
            _session.CurrentReport.SessionEndType.Should().Be(SessionEndType.Paused);
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
        }


        [Fact]
        public async void Stop_when_called_after_pause_returns()
        {
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;

            SetupEpochEndHandler(_session);
            var t = _session.Start();
            try
            {
                await _session.Pause();
            }
            catch (TrainingCanceledException)
            {
            }
            
            Time.TimeProvider = () => DateTime.Now.Add(TimeSpan.FromMinutes(2));

            await _session.Stop();

            _session.Paused.Should().BeFalse();
            _session.Stopped.Should().BeTrue();
            _session.Started.Should().BeFalse();
            _session.CurrentReport.SessionEndType.Should().Be(SessionEndType.Stopped);
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(2);
        }


        [Fact]
        public async void Stop_when_not_started_returns_report()
        {
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 100
            };

            _appState.ActiveSession.TrainingParameters = param;

    
            await _session.Stop();

            _session.Paused.Should().BeFalse();
            _session.Stopped.Should().BeTrue();
            _session.Started.Should().BeFalse();
            _session.CurrentReport.SessionEndType.Should().Be(SessionEndType.Stopped);
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
        }
    
    
        [Fact]
        public async void When_max_epochs_reached_returns_valid_report()
        {
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 1
            };

            _appState.ActiveSession.TrainingParameters = param;

            var report = await _session.Start();
    
            report.SessionEndType.Should().Be(SessionEndType.MaxEpoch);
            _session.Paused.Should().BeFalse();
            _session.Stopped.Should().BeTrue();
            _session.Started.Should().BeFalse();
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
        }

        [Fact]
        public async void When_max_learning_time_reached_returns_valid_report()
        {
            var param = new TrainingParameters(false)
            {
                MaxEpochs = 1_000_000,
                MaxLearningTime = TimeSpan.FromSeconds(1)
            };

            _appState.ActiveSession.TrainingParameters = param;

            var task = _session.Start();
    
            await Task.Delay(3000);
    
            var report = await task;
    
            report.SessionEndType.Should().Be(SessionEndType.Timeout);
            _session.Stopped.Should().BeFalse();
            _session.Paused.Should().BeTrue();
            _session.Started.Should().BeFalse();
            _appState.ActiveSession!.TrainingReports.Count.Should().Be(1);
        }


        [Fact]
        public void Algorithm_is_changed_when_new_parameters_are_set()
        {
            var param = new TrainingParameters(false)
            {
                Algorithm = TrainingAlgorithm.LevenbergMarquardt,
            };
            _session.Trainer!.Algorithm.Should().BeOfType<GradientDescentAlgorithm>();

            _appState.ActiveSession!.TrainingParameters = param;

            _session.Trainer.Algorithm.Should().BeOfType<LevenbergMarquardtAlgorithm>();
        }

        [Fact]
        public void Sets_in_trainer_are_changed_when_new_sets_are_stored()
        {
            var newData = TrainingDataMocks.RandomTrainingData();
            _session.Trainer!.TrainingSets.Should().NotBeEquivalentTo(newData);
            _appState.ActiveSession!.TrainingData!.StoreNewSets(newData);

            _session.Trainer!.TrainingSets.Should().BeEquivalentTo(newData);
        }

        [Fact]
        public void Sets_in_trainer_are_changed_when_normalization_method_is_changed()
        {
            var newData = TrainingDataMocks.RandomTrainingData();
            _session.Trainer!.TrainingSets.Should().NotBeEquivalentTo(newData);
            _appState.ActiveSession!.TrainingData!.ChangeNormalization(newData, NormalizationMethod.Mean, new MeanNormalization());

            _session.Trainer!.TrainingSets.Should().BeEquivalentTo(newData);
        }
    }
}