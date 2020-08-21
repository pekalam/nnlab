using Data.Application.ViewModels.DataSource.Preview;
using Data.Domain.Services;
using FluentAssertions;
using Infrastructure.Domain;
using NNLib.Common;
using Xunit;

namespace Data.Application.Tests.Application.DataSourcePreview
{
    public class DataSetInstanceAccessorTest
    {
        private TrainingData LoadTrainingData(string fileName, SupervisedSetVariableIndexes variableIndexes)
        {
            var dsService = new SupervisedDataSetService();
            var data = dsService.LoadSets(fileName, new LinearDataSetDivider(), new DataSetDivisionOptions()
            {
                TrainingSetPercent = 100,
            }, variableIndexes);
            return data;
        }

        private static readonly SupervisedSetVariableIndexes InputTarget =
            new SupervisedSetVariableIndexes(new[] {0,}, new[] {1});


        [Fact]
        public void When_constructed_with_valid_data_returns_valid_table()
        {
            var fileName = @"Files/plik.csv";
            TrainingData trainingData = LoadTrainingData(fileName, InputTarget);

            var dataSetInstanceAccessor =
                new DataSetInstanceAccessor(trainingData);

            var instance0 = dataSetInstanceAccessor[0];
            var columnNames = new string[instance0.Columns.Count];
            for (int i = 0; i < instance0.Columns.Count; i++)
            {
                columnNames[i] = instance0.Columns[i].ColumnName;
            }

            columnNames.Should().BeEquivalentTo("x", "y");


            for (int j = 0; j < trainingData.Sets.TrainingSet.Input.Count; j++)
            {
                var instance = dataSetInstanceAccessor[j];
                instance.Rows.Count.Should().Be(1);
                for (int i = 0; i < instance.Columns.Count; i++)
                {
                    (instance.Rows[0][i] as string).Should().Be((j + 1).ToString());
                }
            }


            trainingData.Sets.Dispose();
            trainingData.Sets.Dispose();
        }
    }
}