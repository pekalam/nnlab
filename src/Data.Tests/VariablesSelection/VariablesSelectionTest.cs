﻿using System.Linq;
using Common.Domain;
using Data.Application.Controllers;

using Data.Application.ViewModels;
using Data.Domain.Services;
using FluentAssertions;
using Moq.AutoMock;
using NNLib.Csv;
using TestUtils;
using Xunit;

namespace Data.Application.Tests.VariablesSelection
{
    public class VariablesSelectionTest
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private VariablesSelectionController _ctrl;
        private IVariablesSelectionController _controller;
        private VariablesSelectionViewModel _vm;

        private Session _session;

        public VariablesSelectionTest()
        {
            _mocker.UseTestEa();
            _mocker.UseTestRm();

            _appState = _mocker.UseImpl<AppState>();
            _mocker.UseImpl<ITrainingDataService,TrainingDataService>();
            _session = _appState.CreateSession();
            _session.TrainingData = TrainingDataMocks.ValidData4;

            _ctrl = _mocker.UseImpl<IVariablesSelectionController, VariablesSelectionController>();
            _controller = _ctrl;

            _vm = _mocker.UseVm<VariablesSelectionViewModel>();
            _vm.IsActive = true;
        }


        [Fact]
        public void When_variable_use_changed_training_data_indexes_are_changed_or_error_is_set()
        {
            //trigger validation on change
            foreach (var model in _vm.Variables)
            {
                model.PropertyChanged += (sender, args) => _ = model[args.PropertyName];
            }

            //how many times variables were set
            var called = 0;
            _session.TrainingData.PropertyChanged +=
                (sender, args) => _ = args.PropertyName == nameof(TrainingData.Variables) ? called++ : 0;

            //Input Input Target
            _vm.Variables[1].VariableUse = VariableUses.Input;

            _session.TrainingData.Variables.Indexes.InputVarIndexes.Should().BeEquivalentTo(0, 1);
            _session.TrainingData.Variables.Indexes.TargetVarIndexes.Should().BeEquivalentTo(2);

            //Ignore Input Target
            _vm.Variables[0].VariableUse = VariableUses.Ignore;

            _session.TrainingData.Variables.Indexes.Ignored.Should().BeEquivalentTo(0);
            _session.TrainingData.Variables.Indexes.InputVarIndexes.Should().BeEquivalentTo(1);
            _session.TrainingData.Variables.Indexes.TargetVarIndexes.Should().BeEquivalentTo(2);


            //Ignore Ignore Target (error)
            _vm.Variables[1].VariableUse = VariableUses.Ignore;

            _vm.Error.Should().NotBeNullOrEmpty();

            _session.TrainingData.Variables.Indexes.Ignored.Should().BeEquivalentTo(0);
            _session.TrainingData.Variables.Indexes.InputVarIndexes.Should().BeEquivalentTo(1);
            _session.TrainingData.Variables.Indexes.TargetVarIndexes.Should().BeEquivalentTo(2);

            called.Should().Be(2);


            //previous variables usage cannot trigger trainingdata change
            called = 0;
            //Ignore Input Target
            _vm.Variables[1].VariableUse = VariableUses.Input;

            called.Should().Be(0);

            _session.TrainingData.Variables.Indexes.Ignored.Should().BeEquivalentTo(0);
            _session.TrainingData.Variables.Indexes.InputVarIndexes.Should().BeEquivalentTo(1);
            _session.TrainingData.Variables.Indexes.TargetVarIndexes.Should().BeEquivalentTo(2);
        }

        [Fact]
        public void IgnoreAll_should_ignore_all_variables()
        {
            //trigger validation on change
            foreach (var model in _vm.Variables)
            {
                model.PropertyChanged += (sender, args) => _ = model[args.PropertyName];
            }

            _vm.Controller.IgnoreAllCommand.Execute(null);

            //how many times variables were set
            var called = 0;
            _session.TrainingData.PropertyChanged +=
                (sender, args) => _ = args.PropertyName == nameof(TrainingData.Variables) ? called++ : 0;

            called.Should().Be(0);


            _vm.Variables.Any(v => v.VariableUse != VariableUses.Ignore).Should().BeFalse();
            _vm.Error.Should().NotBeNullOrEmpty();

            _vm.Variables[0].VariableUse = VariableUses.Input;
            _vm.Error.Should().NotBeNullOrEmpty();

            _vm.Variables[1].VariableUse = VariableUses.Target;
            _vm.Error.Should().BeNullOrEmpty();

            called.Should().Be(1);
        }
    }
}
