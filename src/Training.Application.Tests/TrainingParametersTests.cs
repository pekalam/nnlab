using Common.Domain;
using FluentAssertions;
using Moq.AutoMock;
using NNLib.Common;
using NNLib.Data;
using NNLib.Training.GradientDescent;
using NNLib.Training.LevenbergMarquardt;
using System;
using System.Linq;
using TestUtils;
using Training.Application.Controllers;
using Training.Application.ViewModels;
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

            var moduleCtrl = _mocker.UseImpl<ModuleController>();
            moduleCtrl.Run();
        }


        [Fact]
        public void Is_max_learning_time_checked_should_be_false_if_max_learning_time_set()
        {
            var session = _appState.CreateSession();
            session.SetupValidAndGate();
            session.TrainingParameters.MaxLearningTime = TimeSpan.FromMinutes(2);

            var vm = _mocker.UseVm<TrainingParametersViewModel>();
            vm.IsMaxLearningTimeChecked.Should().BeFalse();
            vm.MaxLearningTime.Should().BeAtLeast(TimeSpan.FromMinutes(2));
        }

        [Fact]
        public void Properties_are_set_after_ok_command_is_called()
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

            //create vm after session is created
            _vm = _mocker.UseVm<TrainingParametersViewModel>();

            _moduleState.ActiveSession!.Trainer!.Algorithm.Should().BeOfType<GradientDescentAlgorithm>();

            _vm.TrainingParameters!.Algorithm = TrainingAlgorithm.LevenbergMarquardt;
            _service.OkCommand.Execute();

            _moduleState.ActiveSession.Trainer.Algorithm.Should().BeOfType<LevenbergMarquardtAlgorithm>();

        }
    }
}