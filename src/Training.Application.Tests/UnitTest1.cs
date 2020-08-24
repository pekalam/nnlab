using System;
using System.Reflection;
using Common.Domain;
using Moq.AutoMock;
using Shell.Interface;
using TestUtils;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Domain;
using Xunit;

namespace Training.Application.Tests
{
    public class UnitTest1
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private ModuleController _ctrl;
        private TestEa _ea;
        private ModuleState _moduleState;

        public UnitTest1()
        {
            _ea = _mocker.UseTestEa();
            _mocker.UseTestRm();
            _mocker.UseTestVmAccessor();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState =_mocker.UseImpl<ModuleState>();

            _ctrl = _mocker.CreateInstance<ModuleController>();
        }

        [Fact]
        public void NavItem_when_valid_session_is_created_is_enabled()
        {
            _ea.VerifyTimesCalled<EnableNavMenuItem>(0);
            var session = _appState.CreateSession();
            session.Network = MLPMocks.ValidNet1;
            session.TrainingData = TrainingDataMocks.ValidData1;

            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);
        }
    }
}
