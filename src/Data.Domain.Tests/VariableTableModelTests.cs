using FluentAssertions;
using NNLib.Common;
using TestUtils;
using Xunit;

namespace Data.Application.Tests.Domain.VariableTableModel
{
    public class VariableTableModelTests
    {
        [Fact]
        public void FromTrainingData_constructs_valid_model()
        {
            var model = Data.Domain.VariableTableModel.FromTrainingData(TrainingDataMocks.ValidData4);

            model.Length.Should().Be(3);

            model[0].VariableUse.Should().Be(VariableUses.Input);
            model[1].VariableUse.Should().Be(VariableUses.Target);
            model[2].VariableUse.Should().Be(VariableUses.Target);
        }


        [Fact]
        public void Change_of_variableUse_prop_sets_error()
        {
            int e1=0, e2=0, e3 = 0;
            int c1 = 0, c2 = 0, c3 = 0;
            var model = Data.Domain.VariableTableModel.FromTrainingData(TrainingDataMocks.ValidData4);

            model[0].OnError = s => _ = s != null ? e1++ : c1++;
            model[1].OnError = s => _ = s != null ? e2++ : c2++;
            model[2].OnError = s => _ = s != null ? e3++ : c3++;


            //Target Target Target
            model[0].VariableUse = VariableUses.Target;
            model[0][nameof(Data.Domain.VariableTableModel.VariableUse)].Should().NotBeNullOrEmpty();
            c1.Should().Be(0);
            e1.Should().Be(1);
            c2.Should().Be(0);
            e2.Should().Be(0);
            c3.Should().Be(0);
            e3.Should().Be(0);

            //Ignore Target Target
            model[0].VariableUse = VariableUses.Ignore;
            model[0][nameof(Data.Domain.VariableTableModel.VariableUse)].Should().NotBeNullOrEmpty();
            c1.Should().Be(0);
            e1.Should().Be(2);
            c2.Should().Be(0);
            e2.Should().Be(0);
            c3.Should().Be(0);
            e3.Should().Be(0);


            //Ignore Input Target
            model[1].VariableUse = VariableUses.Input;
            model[0][nameof(Data.Domain.VariableTableModel.VariableUse)].Should().BeNullOrEmpty();
            c1.Should().Be(1);
            e1.Should().Be(2);
            c2.Should().Be(0);
            e2.Should().Be(0);
            c3.Should().Be(0);
            e3.Should().Be(0);

            //Ignore Input Input
            model[2].VariableUse = VariableUses.Input;
            model[2][nameof(Data.Domain.VariableTableModel.VariableUse)].Should().NotBeNullOrEmpty();
            c1.Should().Be(1);
            e1.Should().Be(2);
            c2.Should().Be(0);
            e2.Should().Be(0);
            c3.Should().Be(0);
            e3.Should().Be(1);
        }
    }
}
