using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Data.Domain.Services;
using FluentAssertions;
using NNLib.Common;
using Xunit;

namespace Data.Tests.Domain.SupervisedDataSetServiceTests
{
    public class SupervisedDataSetServiceTests
    {
        [Theory]
        [InlineData(100, 99, 1024 * 1024)]
        [InlineData(100, 99, 16)]
        [InlineData(33, 33, 16)]
        [InlineData(33, 33, 1024 * 1024)]
        public void LoadSets_when_valid_params_returns_valid_trainingData(int trainingSetPercentage, int setCount,
            int pageSize)
        {
            var dsService = new SupervisedDataSetService();

            var fileName = @"Files\plik.csv";

            var trainingData = dsService.LoadSets(fileName, new LinearDataSetDivider(), new DataSetDivisionOptions()
            {
                TrainingSetPercent = trainingSetPercentage,
            }, new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }));


            var trainingSet = trainingData.Sets.TrainingSet;
            trainingSet.Should().NotBeNull();
            trainingSet.Input.Count.Should().Be(setCount);
            trainingSet.Target.Count.Should().Be(setCount);

            for (int i = 0; i < setCount; i++)
            {
                var inp = trainingSet.Input[i];
                var tar = trainingSet.Target[i];


                inp[0, 0].Should().Be(i + 1);
                tar[0, 0].Should().Be(i + 1);
            }

            trainingSet.Input.Dispose();
            trainingSet.Target.Dispose();
        }


        [Theory]
        [InlineData(33, 33, 33, 33, 33, 33, 1024 * 1024)]
        public void LoadSets_when_valid_params_returns_returns_3_divided_sets(int trainingSetPercentage,
            int trainingSetCount, int validationSetPercentage, int validationSetCount, int testSetPercentage,
            int testSetCount, int pageSize)
        {
            var dsService = new SupervisedDataSetService();

            var fileName = @"Files\plik.csv";

            var trainingData = dsService.LoadSets(fileName, new LinearDataSetDivider(), new DataSetDivisionOptions()
            {
                TrainingSetPercent = trainingSetPercentage,
                ValidationSetPercent = validationSetPercentage,
                TestSetPercent = testSetPercentage,
            }, new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }));


            var trainingSet = trainingData.Sets.TrainingSet;
            var validationSet = trainingData.Sets.ValidationSet;
            var testSet = trainingData.Sets.TestSet;
            trainingSet.Should().NotBeNull();
            trainingSet.Input.Count.Should().Be(trainingSetCount);
            trainingSet.Target.Count.Should().Be(trainingSetCount);

            validationSet.Should().NotBeNull();
            validationSet.Input.Count.Should().Be(validationSetCount);
            validationSet.Target.Count.Should().Be(validationSetCount);

            testSet.Should().NotBeNull();
            testSet.Input.Count.Should().Be(testSetCount);
            testSet.Target.Count.Should().Be(testSetCount);

            int lastCsvVal = 0;
            for (int i = 0; i < trainingSetCount; i++)
            {
                var inp = trainingSet.Input[i];
                var tar = trainingSet.Target[i];


                inp[0, 0].Should().Be(i + 1);
                tar[0, 0].Should().Be(i + 1);
                lastCsvVal = i + 1;
            }


            for (int i = 0; i < validationSetCount; i++)
            {
                var inp = validationSet.Input[i];
                var tar = validationSet.Target[i];


                inp[0, 0].Should().Be(lastCsvVal + 1);
                tar[0, 0].Should().Be(lastCsvVal + 1);
                lastCsvVal = lastCsvVal + 1;
            }


            for (int i = 0; i < testSetCount; i++)
            {
                var inp = testSet.Input[i];
                var tar = testSet.Target[i];


                inp[0, 0].Should().Be(lastCsvVal + 1);
                tar[0, 0].Should().Be(lastCsvVal + 1);
                lastCsvVal = lastCsvVal + 1;
            }

            trainingSet.Input.Dispose();
            trainingSet.Target.Dispose();
        }

        [Fact]
        public void LoadDefaultSet_when_valid_params_returns_valid_default_trainingData()
        {
            var dsService = new SupervisedDataSetService();

            var fileName = @"Files\plik.csv";

            var trainingData = dsService.LoadDefaultSet(fileName);
            trainingData.Variables.Names.Select(v => (string)v).Should().BeEquivalentTo("x", "y");
            trainingData.Variables.Indexes.InputVarIndexes.Should().BeEquivalentTo(0);
            trainingData.Variables.Indexes.TargetVarIndexes.Should().AllBeEquivalentTo(1);

            trainingData.Sets.ValidationSet.Should().BeNull();
            trainingData.Sets.TestSet.Should().BeNull();
            var trainingSet = trainingData.Sets.TrainingSet;
            trainingSet.Should().NotBeNull();
            trainingSet.Input.Count.Should().Be(99);
            trainingSet.Target.Count.Should().Be(99);

            for (int i = 0; i < 99; i++)
            {
                var inp = trainingSet.Input[i];

                inp.RowCount.Should().Be(1);
                inp.ColumnCount.Should().Be(1);

                inp[0, 0].Should().Be(i + 1);
            }


            for (int i = 0; i < 99; i++)
            {
                var tar = trainingSet.Target[i];

                tar.RowCount.Should().Be(1);
                tar.ColumnCount.Should().Be(1);

                tar[0, 0].Should().Be(i + 1);
            }

            trainingSet.Input.Dispose();
        }

        private void CheckFileIsNotLocked(string fileName)
        {
            //not throws
            using (var fs = File.OpenRead(fileName))
            {
            }
        }


        [Fact]
        public void LoadSetFiles_when_valid_csv_file_reads_files_into_sets()
        {
            var dsService = new SupervisedDataSetService();

            var files = new[] { @"Files\plik_t.csv", @"Files\plik_v.csv", @"Files\plik_ts.csv" };

            var trainingData = dsService.LoadDefaultSetsFromFiles(trainingSetFile: files[0], validationSetFile: files[1], testSetFile: files[2]);
            trainingData.Variables.Names.Select(v => (string)v).Should().BeEquivalentTo("x", "y");
            trainingData.Variables.Indexes.InputVarIndexes.Should().BeEquivalentTo(0);
            trainingData.Variables.Indexes.TargetVarIndexes.Should().BeEquivalentTo(1);

            var trainingSet = trainingData.Sets.TrainingSet;
            trainingSet.Should().NotBeNull();
            trainingSet.Input.Count.Should().Be(30);
            trainingSet.Target.Count.Should().Be(30);


            var validationSet = trainingData.Sets.ValidationSet;
            validationSet.Should().NotBeNull();
            validationSet.Input.Count.Should().Be(20);
            validationSet.Target.Count.Should().Be(20);


            trainingData.Sets.Dispose();
        }


        [Fact]
        public void ChangeVariables_when_called_changes_vector_sets_indices()
        {
            var dsService = new SupervisedDataSetService();
            var fileName = @"Files\testxyz.csv";


            var loadedData = dsService.LoadSets(fileName, new LinearDataSetDivider(), new DataSetDivisionOptions()
            {
                TrainingSetPercent = 33,
                ValidationSetPercent = 33,
                TestSetPercent = 33
            }, new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1, 2 }));


            loadedData.Sets.TrainingSet.Input[0].RowCount.Should().Be(1);
            loadedData.Sets.TrainingSet.Input[0][0, 0].Should().Be(1);
            loadedData.Sets.TrainingSet.Target[0].RowCount.Should().Be(2);
            loadedData.Sets.TrainingSet.Target[0][0, 0].Should().Be(1);
            loadedData.Sets.TrainingSet.Target[0][1, 0].Should().Be(1);


            var newVariables = new SupervisedSetVariableIndexes(new[] { 0, 1 }, new[] { 2 });

            loadedData = dsService.ChangeVariables(newVariables, loadedData);


            loadedData.Sets.TrainingSet.Input[0].RowCount.Should().Be(2);
            loadedData.Sets.TrainingSet.Input[0][0, 0].Should().Be(1);
            loadedData.Sets.TrainingSet.Input[0][1, 0].Should().Be(1);
            loadedData.Sets.TrainingSet.Target[0].RowCount.Should().Be(1);

        }
    }
}
