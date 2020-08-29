using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Common.Domain;
using FakeItEasy;
using FluentAssertions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using TestUtils;
using Xunit;

namespace Common.Tests
{
    public class AppStateTests
    {
        private AppState appState = new AppState();
        private int _activeSessionChangedCalled;

        public AppStateTests()
        {
            appState.ActiveSessionChanged += (_, __) => _activeSessionChangedCalled++;
        }

        [Fact]
        public void When_created_there_is_no_active_sessions()
        {
            //assert
            appState.ActiveSession.Should().BeNull();
            appState.Sessions.Count.Should().Be(0);
        }

        [Fact]
        public void CreatedSessions_distinct_names()
        {
            
            appState.CreateSession(); 
            appState.CreateSession();

            //assert
            appState.Sessions.Select(v => v.Name).Distinct().Should().HaveCount(2);
        }

        [Fact]
        public void CreateSession_when_0_sessions_calls_property_changed_event()
        {
            //arrange
            
            appState.PropertyChanged += (sender, args) =>
                args.PropertyName.Should().Be(nameof(AppState.ActiveSession));

            //act
            var session = appState.CreateSession();


            //assert
            appState.Sessions.Count.Should().Be(1);
            appState.ActiveSession.Should().Be(session);
            _activeSessionChangedCalled.Should().Be(1);
        }


        [Fact]
        public void SetActive_calls_property_changed_event()
        {
            int times = 0;

            //arrange
            
            appState.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AppState.ActiveSession))
                    times++;
            };
            appState.CreateSession(); //active session changed
            var session1 = appState.CreateSession(); //does not call

            //act
            appState.ActiveSession = session1; //active session changed

            //assert
            appState.ActiveSession.Should().Be(session1);
            times.Should().Be(2);
            _activeSessionChangedCalled.Should().Be(2);
        }


        [Fact]
        public void DuplicateActive_creates_deep_copy_of_active_session_and_sets_as_active()
        {
            //arrange

            int times = 0;
            
            appState.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AppState.ActiveSession))
                    times++;
            };

            var session = appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidData3;
            session.TrainingParameters = new Fixture().Create<TrainingParameters>();

            //act
            var dup = appState.DuplicateActiveSession();

            //assert
            times.Should().Be(2);
            appState.Sessions.Count.Should().Be(2);
            appState.ActiveSession.Should().Be(dup);
            _activeSessionChangedCalled.Should().Be(2);


            dup.TrainingData.Should().NotBe(session.TrainingData).And.Should().NotBeNull();
            dup.TrainingParameters.Should().NotBe(session.TrainingParameters).And.Should().NotBeNull();
        }

        [Fact]
        public void DuplicateActive_throws_if_0_sessions()
        {
            Assert.ThrowsAny<Exception>(() => appState.DuplicateActiveSession());
        }
    }


    public class SessionTests
    {
        private AppState appState = new AppState();

        [Fact]
        public void Unload_data_sets_null_training_data()
        {
            var session = appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidData1;

            session.UnloadTrainingData();
            session.TrainingData.Should().BeNull();
        }

        [Fact]
        public void TrainingParameters_when_data_and_network_are_set_is_created()
        {
            var session = appState.CreateSession();
            session.TrainingParameters.Should().BeNull();
            session.TrainingData = TrainingDataMocks.ValidData1;
            session.Network = MLPMocks.ValidNet1;


            session.TrainingParameters.Should().NotBeNull();
        }
    }


    public class SessionReportsCollectionTetsts
    {
        private TrainingReportsCollection collection = new TrainingReportsCollection();

        [Fact]
        public void Add_when_overlapping_added_throws()
        {
            var paused = TrainingSessionReport.CreatePausedSessionReport(10, 0.1, Time.Now.Subtract(TimeSpan.FromHours(1)), new List<EpochEndArgs>());
            Time.TimeProvider = () => DateTime.Now.AddMinutes(5);
            var stopped = TrainingSessionReport.CreateStoppedSessionReport(10, 0.1, Time.Now.Subtract(TimeSpan.FromHours(1)), new List<EpochEndArgs>());
            
            collection.Add(paused);
            
            Assert.ThrowsAny<Exception>(() => collection.Add(stopped));
        }

        [Fact]
        public void Add_when_last_is_of_terminating_type_throws()
        {
            var paused = TrainingSessionReport.CreatePausedSessionReport(10, 0.1, Time.Now.Subtract(TimeSpan.FromHours(1)), new List<EpochEndArgs>());
            Time.TimeProvider = () => DateTime.Now.AddHours(1);
            var stopped = TrainingSessionReport.CreateStoppedSessionReport(10, 0.1, Time.Now.Subtract(TimeSpan.FromHours(1)), new List<EpochEndArgs>());
            Time.TimeProvider = () => DateTime.Now.AddHours(1);
            var timeout = TrainingSessionReport.CreateTimeoutSessionReport(10, 0.1, Time.Now.Subtract(TimeSpan.FromHours(1)), new List<EpochEndArgs>());
            
            collection.Add(paused);
            collection.Add(stopped);
            
            Assert.ThrowsAny<Exception>(() => collection.Add(timeout));
        }
        
    }

}