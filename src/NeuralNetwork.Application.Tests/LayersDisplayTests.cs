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
        private AppState _appState;
        private ModuleState _moduleState;
        private LayersDisplayViewModel _vm;
        private LayersDisplayService _service;

        public LayersDisplayTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _mocker.UseTestVmAccessor();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();
            _mocker.UseImpl<NeuralNetworkService>();
            _mocker.UseImpl<INeuralNetworkService, NNControlNeuralNetworkServiceDecorator>();
            _mocker.UseImpl<NeuralNetworkShellController>();
            _mocker.UseImpl<ITransientController<LayersDisplayService>, LayersDisplayController>();
            _service = _mocker.UseImpl<ILayersDisplayService, LayersDisplayService>();


        }

        [Fact]
        public void Layers_when_no_active_session_are_empty()
        {
               
            _vm = _mocker.UseVm<LayersDisplayViewModel>();

            _vm.Layers.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Layers_when_no_neural_network_in_active_session_are_empty()
        {
            
            _appState.CreateSession();

            _vm = _mocker.UseVm<LayersDisplayViewModel>();

            _vm.Layers.Should().BeNullOrEmpty();
        }

        [Fact]
        public void Layers_change_when_network_in_active_session_is_changed_after_vm_is_created()
        {
            
            var session = _appState.CreateSession();

            _vm = _mocker.UseVm<LayersDisplayViewModel>();

            session.Network = MLPMocks.ValidNet1;

            _vm.Layers.Should().HaveCount(MLPMocks.ValidNet1.TotalLayers + 1);

            session.Network = MLPMocks.ValidNet2;

            _vm.Layers.Should().HaveCount(MLPMocks.ValidNet2.TotalLayers + 1);
        }

        [Fact]
        public void Layers_change_when_network_in_active_session_is_changed_before_vm_is_created()
        {
            
            var session = _appState.CreateSession();

            session.Network = MLPMocks.ValidNet1;

            _vm = _mocker.UseVm<LayersDisplayViewModel>();

            _vm.Layers.Should().HaveCount(MLPMocks.ValidNet1.TotalLayers + 1);

            session.Network = MLPMocks.ValidNet2;

            _vm.Layers.Should().HaveCount(MLPMocks.ValidNet2.TotalLayers + 1);
        }

        [Fact]
        public void Layers_change_when_active_session_is_changed()
        {
            var session = _appState.CreateSession();
            session.Network = MLPMocks.ValidNet1;

            _vm = _mocker.UseVm<LayersDisplayViewModel>();
            _vm.Layers.Should().HaveCount(_appState.ActiveSession.Network!.TotalLayers + 1);

            var session2 = _appState.CreateSession();
            session2.Network = MLPMocks.ValidNet1;
            _appState.ActiveSession = session2;

            _vm.Layers.Should().HaveCount(_appState.ActiveSession.Network!.TotalLayers + 1);
        }
    }
}