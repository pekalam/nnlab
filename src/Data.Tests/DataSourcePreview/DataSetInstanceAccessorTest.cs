using Common.Domain;
using Data.Application.ViewModels;
using Data.Application.ViewModels.DataSourcePreview;
using Data.Domain.Services;
using FluentAssertions;
using NNLib.Common;
using Xunit;

namespace Data.Application.Tests.DataSourcePreview
{
    public class DataSetInstanceAccessorTest
    {
        private TrainingData LoadTrainingData(string fileName, SupervisedSetVariableIndexes variableIndexes)
        {
            var dsService = new TrainingDataService();
            var data = dsService.LoadDefaultTrainingData(fileName);
            return data;
        }

        private static readonly SupervisedSetVariableIndexes InputTarget =
            new SupervisedSetVariableIndexes(new[] {0,}, new[] {1});


        [Fact]
        public void When_constructed_with_valid_data_returns_valid_table()
        {
            var fileName = @"Files/plik.csv";
            var appState = new AppState();
            TrainingData trainingData = LoadTrainingData(fileName, InputTarget);
            appState.CreateSession().TrainingData = trainingData;

            var dataSetInstanceAccessor =
                new DataSetInstanceAccessor(appState);

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