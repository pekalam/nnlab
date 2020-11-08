using System;
using System.Linq;
using Common.Domain;
using Common.Framework;
using FluentAssertions;
using Moq.AutoMock;
using NNLib;
using NNLib.Common;
using NNLib.Data;
using NNLib.Training.GradientDescent;
using NNLib.Training.LevenbergMarquardt;
using TestUtils;
using Training.Application.Controllers;
using Training.Application.ViewModels;
using Training.Domain;
using Xunit;

namespace Training.Application.Tests
{
    public class TrainingParametersTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private ModuleState _moduleState;
        private TrainingParametersViewModel _vm;
        private TrainingParametersController _ctrl;
        private TrainingParametersController _service;
        
        public TrainingParametersTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();
            _ctrl = _mocker.UseImpl<ITrainingParametersController,TrainingParametersController>();
            _service = _ctrl;


            _vm = _mocker.UseVm<TrainingParametersViewModel>();
        }

        [Fact]
        public void Properties_are_changed_with_active_session()
        {
            var session = _appState.CreateSession();
            session.SetupValidAndGate();

            _vm.TrainingParameters.Should().Be(session.TrainingParameters);

            _vm.IsMaxLearningTimeChecked = false;
            _vm.MaxLearningTime = Time.Now.AddMinutes(2);

            var session2 = _appState.CreateSession();
            session2.SetupValidAndGate();

            _appState.ActiveSession = session2;

            _vm.IsMaxLearningTimeChecked.Should().BeTrue();
            _vm.MaxLearningTime.Should().Be(default);


            _appState.ActiveSession = session;
            _vm.IsMaxLearningTimeChecked.Should().BeFalse();
            _vm.MaxLearningTime.Should().BeAtLeast(TimeSpan.FromMinutes(2));

        }

        [Fact]
        public void Properties_are_set_when_changed_before_vm_created()
        {
            var session = _appState.CreateSession();
            session.SetupValidAndGate();
            session.TrainingParameters.MaxLearningTime = TimeSpan.FromMinutes(2);

            var vm = _mocker.UseVm<TrainingParametersViewModel>();
            vm.IsMaxLearningTimeChecked.Should().BeFalse();
            vm.MaxLearningTime.Should().BeAtLeast(TimeSpan.FromMinutes(2));
        }

        [Fact]
        public void f()
        {


            var session = _appState.CreateSession();
            session.TrainingData = new TrainingData(new SupervisedTrainingData(SupervisedTrainingSamples.FromArrays(
               Enumerable.Repeat(new[] { 0d, 1d }, 1000).ToArray(),
              Enumerable.Repeat(new[] { 1d }, 1000).ToArray()
            )), new SupervisedTrainingSamplesVariables(new SupervisedSetVariableIndexes(new []{0}, new []{1}), new VariableName[]
            {
                new VariableName("x"), new VariableName("y"), 
            } ), TrainingDataSource.Memory, NormalizationMethod.None);
            session.Network = MLPMocks.AndGateNet;
            _moduleState.ActiveSession.Trainer.Algorithm.Should().BeOfType<GradientDescentAlgorithm>();

            _vm.TrainingParameters.Algorithm = TrainingAlgorithm.LevenbergMarquardt;
            _service.OkCommand.Execute();

            _moduleState.ActiveSession.Trainer.Algorithm.Should().BeOfType<LevenbergMarquardtAlgorithm>();

        }
    }
}