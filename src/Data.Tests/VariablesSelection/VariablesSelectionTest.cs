using Common.Domain;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.VariablesSelection;
using Data.Domain;
using Data.Domain.Services;
using FluentAssertions;
using Moq.AutoMock;
using NNLib.Common;
using TestUtils;
using Xunit;

namespace Data.Application.Tests.VariablesSelection
{
    public class VariablesSelectionTest
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private VariablesSelectionController _ctrl;
        private IVariablesSelectionService _service;
        private VariablesSelectionViewModel _vm;

        private Session _session;

        public VariablesSelectionTest()
        {
            _mocker.UseTestRm();

            _appState = _mocker.UseImpl<AppState>();
            _mocker.UseImpl<ModuleState>();
            _mocker.UseImpl<ITrainingDataService,TrainingDataService>();
            _session = _appState.CreateSession();
            _session.TrainingData = TrainingDataMocks.ValidData4;

            _ctrl = _mocker.UseImpl<IVariablesSelectionService, VariablesSelectionController>();
            _service = _ctrl;

            _vm = _mocker.CreateInstance<VariablesSelectionViewModel>();
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
    }
}
