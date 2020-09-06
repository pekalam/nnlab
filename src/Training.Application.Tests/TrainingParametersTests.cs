using System;
using Common.Domain;
using Common.Framework;
using FluentAssertions;
using Moq.AutoMock;
using NNLib;
using TestUtils;
using Training.Application.Controllers;
using Training.Application.Services;
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
        private TrainingParametersService _service;
        
        public TrainingParametersTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _mocker.UseTestVmAccessor();
            _appState = _mocker.UseImpl<AppState>();
            _moduleState = _mocker.UseImpl<ModuleState>();
            _ctrl = _mocker.UseImpl<ITransientController<TrainingParametersService>,TrainingParametersController>();
            _service = _mocker.UseImpl<ITrainingParametersService, TrainingParametersService>();


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
            session.SetupValidAndGate();
            _moduleState.ActiveSession.Trainer.Algorithm.Should().BeOfType<GradientDescentAlgorithm>();

            _vm.TrainingParameters.Algorithm = TrainingAlgorithm.LevenbergMarquardt;
            _service.OkCommand.Execute();

            _moduleState.ActiveSession.Trainer.Algorithm.Should().BeOfType<LevenbergMarquardtAlgorithm>();

        }
    }
}