using Common.Domain;
using Common.Framework;
using FluentAssertions;
using Moq.AutoMock;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using TestUtils;
using Xunit;

namespace NeuralNetwork.Application.Tests
{
    public class LayersDisplayTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private ModuleController _moduleController;
        private AppState _appState;
        private ModuleState _moduleState;
        private LayersDisplayViewModel _vm;

        public LayersDisplayTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();
            _mocker.UseImpl<NeuralNetworkService>();
            _mocker.UseImpl<INeuralNetworkService, NNControlNeuralNetworkServiceDecorator>();
            _moduleController = _mocker.UseImpl<ModuleController>();
            _mocker.UseImpl<NeuralNetworkShellController>();
            _mocker.UseImpl<ILayersDisplayController, LayersDisplayController>();

            _moduleController.Run();
        }

        [Fact]
        public void Layers_when_no_active_session_are_empty()
        {
            _vm = _mocker.UseVm<LayersDisplayViewModel>();

            _vm.Layers.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Layers_change_when_active_session_is_changed()
        {
            var session = _appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidData1;
            session.Network = MLPMocks.ValidNet1;

            _vm = _mocker.UseVm<LayersDisplayViewModel>();
            _vm.Layers.Should().HaveCount(_appState.ActiveSession.Network!.TotalLayers + 1);

            var session2 = _appState.CreateSession();
            session2.TrainingData = TrainingDataMocks.ValidData1;
            session2.Network = MLPMocks.ValidNet1;
            _appState.ActiveSession = session2;

            _vm.Layers.Should().HaveCount(_appState.ActiveSession.Network!.TotalLayers + 1);
        }
    }
}