using Common.Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using TestUtils;
using Xunit;

namespace Training.Domain.Tests
{
    public class ModuleStateTests
    {
        AppState _appState;
        ModuleState _moduleState;

        public ModuleStateTests()
        {
            _appState = new AppState();
            _moduleState = new ModuleState(_appState);
        }


        [Fact]
        public void Test1()
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
        }
    }
}
