using System;
using Common.Domain;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using NNLib.Common;
using Prism.Regions;
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
        private Mock<IRegionManager> _rm;

        public ModuleControllerTests()
        {
            (_rm, _) = _mocker.UseTestRm();
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
        public void State_when_session_with_data_is_created_is_set()
        {
            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            _ctrl.Run();

            _appState.ActiveSession.Network.Should().NotBeNull();
            _moduleState.ModelAdapter.Should().NotBeNull();
        }

        [Fact]
        public void State_when_session_without_data_is_created_is_set_after_data_is_set()
        {
            var sesion = _appState.CreateSession();

            _appState.ActiveSession.Network.Should().BeNull();
            _moduleState.ModelAdapter.Should().BeNull();

            _ctrl.Run();

            _appState.ActiveSession.Network.Should().BeNull();
            _moduleState.ModelAdapter.Should().BeNull();

            //act
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            //assert
            _appState.ActiveSession.Network.Should().NotBeNull();
            _moduleState.ModelAdapter.Should().NotBeNull();
        }

        [Fact]
        public void State_when_no_active_session_is_created_is_set_after_new_session_with_data_is_created()
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
        public void Session_when_created_has_network_parameters_compatible_with_data()
        {
            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            _ctrl.Run();


            var network = _appState.ActiveSession.Network;

            network.Layers[0].InputsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Input[0].RowCount);
            network.Layers[^1].NeuronsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Target[0].RowCount);
        }

        [Fact]
        public void Session_when_created_after_ctrl_run_and_changed_has_network_parameters_compatible_with_data()
        {
            _ctrl.Run();

            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            var firstNet = _appState.ActiveSession.Network;
            var network = _appState.ActiveSession.Network;
            network.Layers[0].InputsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Input[0].RowCount);
            network.Layers[^1].NeuronsCount.Should().Be(TrainingDataMocks.ValidData1.Sets.TrainingSet.Target[0].RowCount);


            var sesion2 = _appState.CreateSession();
            _appState.ActiveSession = sesion2;
            sesion2.TrainingData = TrainingDataMocks.ValidData4;

            network = _appState.ActiveSession.Network;
            network.Layers[0].InputsCount.Should().Be(TrainingDataMocks.ValidData4.Sets.TrainingSet.Input[0].RowCount);
            network.Layers[^1].NeuronsCount.Should().Be(TrainingDataMocks.ValidData4.Sets.TrainingSet.Target[0].RowCount);


            //should not be recreated
            _appState.ActiveSession = sesion;
            _appState.ActiveSession.Network.Should().BeSameAs(firstNet);
        }

        [Fact]
        public void When_session_is_created_before_run_sends_enabled_navitem()
        {
            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;
            _ctrl.Run();
            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);            
        }

        [Fact]
        public void When_session_is_created_after_run_sends_enabled_navitem()
        {
            _ctrl.Run();
            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;
            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);
        }

        [Fact]
        public void When_new_session_without_data_is_created__sends_disabled()
        {
            _ctrl.Run();
            _appState.CreateSession();
            _ea.VerifyTimesCalled<DisableNavMenuItem>(1);

            _appState.ActiveSession = _appState.CreateSession();
            _ea.VerifyTimesCalled<DisableNavMenuItem>(2);
        }

        [Fact]
        public void When_new_active_session_is_changed_sends_enabled_disabled()
        {
            _ctrl.Run();
            var session = _appState.CreateSession();
            _ea.VerifyTimesCalled<DisableNavMenuItem>(1);

            _appState.ActiveSession = _appState.CreateSession();

            _ea.VerifyTimesCalled<DisableNavMenuItem>(2);

            _appState.ActiveSession = session;
            session.TrainingData = TrainingDataMocks.ValidFileData4;
            _ea.VerifyTimesCalled<EnableNavMenuItem>(1);
        }

        [Fact]
        public void When_variables_are_changed_new_network_is_cretated()
        {
            _ctrl.Run();
            var session = _appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidFileData4;


            //act
            session.TrainingData.Variables = new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(
                new []{0}, new []{1}, new []{2}
                ), new []{new VariableName("x"), new VariableName("y"), new VariableName("z")});


            //assert
            var network = _appState.ActiveSession.Network;
            network.Layers[0].InputsCount.Should().Be(1);
            network.Layers[^1].NeuronsCount.Should().Be(1);
        }


        [Fact]
        public void When_session_is_duplicated_with_no_net_network_is_created()
        {
            var session = _appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidData1;
            session.Network = MLPMocks.ValidNet1;

            _ctrl.Run();

            _appState.DuplicateActiveSession(DuplicateOptions.NoTrainingParams | DuplicateOptions.NoNetwork);

            _appState.ActiveSession.Network.Should().NotBeNull().And.NotBeSameAs(session.Network);
        }


        // [Fact]
        // public void f()
        // {
        //     _ctrl.Run();
        //
        //
        //
        //     _ea.GetEvent<PreviewCheckNavMenuItem>().Publish(new PreviewCheckNavMenuItemArgs(-1, ModuleIds.NeuralNetwork));
        //     _rm.VerifyContentNavigation("NeuralNetworkShellView", Times.Once());
        //
        //     _ea.GetEvent<ReloadContentForSession>().Publish((ModuleIds.Data));
        // }
    }


    public class ModuleStateTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private ModuleState _moduleState;
        private ModuleController _controller;


        public ModuleStateTests()
        {
            _mocker.UseTestEa();
            _mocker.UseTestRm();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();
            _mocker.UseImpl<INeuralNetworkService, NeuralNetworkService>();
            _controller = _mocker.UseImpl<ModuleController>();

            _controller.Run();
        }

        [Fact]
        public void ModelAdapter_when_state_created_after_session_changed_event_creates_new_adpater_when_getter_invoked()
        {
            //arrange
            var session = _appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidData1;
            session.Network = MLPMocks.ValidNet1;


            //assert
            _moduleState.ModelAdapter.Should().NotBeNull();
        }

        [Fact]
        public void State_when_session_duplicated_sets_new_model_adapter()
        {
            //arrange
            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            var adapter = _moduleState.ModelAdapter;
            //additional check
            adapter.Should().NotBeNull();

            //act
            _appState.DuplicateActiveSession();
            //additional check
            _moduleState.ModelAdapter.Should().NotBeNull();

            //assert
            _moduleState.ModelAdapter.Should().NotBe(adapter);
        }

        [Fact]
        public void PropertyChanged_event_is_sent_after_active_session_is_set()
        {
            int called = 0;
            _moduleState.PropertyChanged += (sender, args) =>
                called += (args.PropertyName == nameof(ModuleState.ModelAdapter) ? 1 : 0);

            var sesion = _appState.CreateSession();
            sesion.TrainingData = TrainingDataMocks.ValidData1;

            called.Should().Be(1);

            var sesion2 = _appState.CreateSession();
            sesion2.TrainingData = TrainingDataMocks.ValidData1;
            sesion2.Network = MLPMocks.ValidNet1;
            _appState.ActiveSession = sesion2;

            called.Should().Be(2);


            _appState.ActiveSession = sesion;


            called.Should().Be(3);

        }
    }

}
 
