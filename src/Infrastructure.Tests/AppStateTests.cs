using System;
using AutoFixture;
using Common.Domain;
using FluentAssertions;
using TestUtils;
using Xunit;

namespace Common.Tests
{
    public class AppStateTests
    {
        [Fact]
        public void When_created_session_manager_has_no_active_sessions()
        {
            //arrange & act
            var appState = new AppState();

            //assert
            appState.SessionManager.ActiveSession.Should().BeNull();
            appState.SessionManager.Sessions.Count.Should().Be(0);
        }

        [Fact]
        public void CreateSession_when_0_sessions_calls_property_changed_event()
        {
            //arrange
            var appState = new AppState();
            appState.SessionManager.PropertyChanged += (sender, args) =>
                args.PropertyName.Should().Be(nameof(SessionManager.ActiveSession));

            //act
            var session = appState.SessionManager.Create();


            //assert
            appState.SessionManager.Sessions.Count.Should().Be(1);
            appState.SessionManager.ActiveSession.Should().Be(session);
        }


        [Fact]
        public void SetActive_calls_property_changed_event()
        {
            int times = 0;

            //arrange
            var appState = new AppState();
            appState.SessionManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SessionManager.ActiveSession))
                    times++;
            };
            var session1 = appState.SessionManager.Create(); //active session changed
            appState.SessionManager.Create(); //does not call

            //act
            appState.SessionManager.SetActive(session1); //active session changed

            //assert
            appState.SessionManager.ActiveSession.Should().Be(session1);
            times.Should().Be(2);
        }


        [Fact]
        public void DuplicateActive_creates_deep_copy_of_active_session_and_sets_as_active()
        {
            //arrange

            int times = 0;
            var appState = new AppState();
            appState.SessionManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SessionManager.ActiveSession))
                    times++;
            };

            var session = appState.SessionManager.Create();
            session.TrainingData = TrainingDataMocks.ValidData3;
            session.TrainingParameters = new Fixture().Create<TrainingParameters>();

            //act
            var dup = appState.SessionManager.DuplicateActive();

            //assert
            times.Should().Be(2);
            appState.SessionManager.Sessions.Count.Should().Be(2);
            appState.SessionManager.ActiveSession.Should().Be(dup);


            dup.TrainingData.Should().NotBe(session.TrainingData).And.Should().NotBeNull();
            dup.TrainingParameters.Should().NotBe(session.TrainingParameters).And.Should().NotBeNull();
        }

        [Fact]
        public void DuplicateActive_throws_if_0_sessions()
        {
            var appState = new AppState();
            Assert.ThrowsAny<Exception>(() => appState.SessionManager.DuplicateActive());
        }
    }
}