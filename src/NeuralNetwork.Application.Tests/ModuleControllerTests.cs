using System;
using Common.Domain;
using FluentAssertions;
using Moq.AutoMock;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Domain;
using Shell.Interface;
using TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace NeuralNetwork.Application.Tests
{
    public class ModuleControllerTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private ModuleState _moduleState;
        private ModuleController _ctrl;
        private TestEa _ea;

        public ModuleControllerTests()
        {
            _mocker.UseTestRm();
            _ea = _mocker.UseTestEa();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();
            _mocker.UseImpl<NeuralNetworkService>();
            _mocker.UseImpl<INeuralNetworkService, NNControlNeuralNetworkServiceDecorator>();
            _mocker.UseImpl<NeuralNetworkShellController>();
            _mocker.UseImpl<NetDisplayController>();

            _ctrl = _mocker.CreateInstance<ModuleController>();
        }

        [Fact]
        public void State_when_module_not_started_does_not_change()
        {
            //session is created before module start
            var sesion = _appState.CreateSession();

            _appState.ActiveSession.Network.Should().BeNull();
            _moduleState.ModelAdapter.Should().BeNull();
        }

        [Fact]
        public void State_when_session_with_data_is_created_gets_set()
        {
            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            _ctrl.Run();

            _appState.ActiveSession.Network.Should().NotBeNull();
            _moduleState.ModelAdapter.Should().NotBeNull();
        }

        [Fact]
        public void State_when_session_without_data_is_created_after_data_is_set()
        {
            var sesion = _appState.CreateSession();

            _appState.ActiveSession.Network.Should().BeNull();
            _moduleState.ModelAdapter.Should().BeNull();

            _ctrl.Run();

            sesion.TrainingData = TrainingDataMocks.ValidData1;

            _appState.ActiveSession.Network.Should().NotBeNull();
            _moduleState.ModelAdapter.Should().NotBeNull();
        }

        [Fact]
        public void State_when_no_active_session_is_created_after_session_and_data_is_set()
        {
            _ctrl.Run();

            _appState.ActiveSession.Should().BeNull();
            _moduleState.ModelAdapter.Should().BeNull();

            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            _appState.ActiveSession.Network.Should().NotBeNull();
            _moduleState.ModelAdapter.Should().NotBeNull();
        }


        [Fact]
        public void Session_network_when_created_has_parameters_compatible_with_data()
        {
            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            _ctrl.Run();


            var network = _appState.ActiveSession.Network;

            network.Layers[0].InputsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Input[0].RowCount);
            network.Layers[^1].NeuronsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Target[0].RowCount);
        }

        [Fact]
        public void Session_network_when_created_after_ctrl_run_has_parameters_compatible_with_data()
        {
            _ctrl.Run();

            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            var network = _appState.ActiveSession.Network;

            network.Layers[0].InputsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Input[0].RowCount);
            network.Layers[^1].NeuronsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Target[0].RowCount);
        }

        [Fact]
        public void When_session_is_created_before_run_sends_enabled_navitem()
        {
            var sesion = _appState.CreateSession();
            _ctrl.Run();
            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);            
        }

        [Fact]
        public void When_session_is_created_after_run_sends_enabled_navitem()
        {
            _ctrl.Run();
            var sesion = _appState.CreateSession();
            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);
        }
    }
}
