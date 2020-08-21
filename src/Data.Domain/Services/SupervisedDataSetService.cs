using System;
using System.Linq;
using Common.Domain;
using Infrastructure.Domain;
using NNLib.Common;
using NNLib.Csv;

namespace Data.Domain.Services
{
    public interface ISupervisedDataSetService
    {
        TrainingData LoadDefaultSet(string fileName);
        TrainingData LoadSets(string fileName, IDataSetDivider? divider, DataSetDivisionOptions? divisionOptions, SupervisedSetVariableIndexes? variableIndexes);
        TrainingData LoadDefaultSetsFromFiles(string trainingSetFile, string validationSetFile, string testSetFile);
        TrainingData ChangeVariables(SupervisedSetVariableIndexes newVariableIndexes, TrainingData trainingData);
    }

    internal class SupervisedDataSetService : ISupervisedDataSetService
    {
        public TrainingData LoadDefaultSet(string fileName) => LoadSets(fileName, null, null, null);

        public TrainingData LoadSets(string fileName, IDataSetDivider? divider, DataSetDivisionOptions? divisionOptions, SupervisedSetVariableIndexes? variableIndexes)
        {
            var (sets, variableNames, ind) = CsvFacade.LoadSets(fileName, divider, divisionOptions, variableIndexes);
            variableIndexes = ind;

            var variables = new SupervisedSetVariables(variableIndexes, Enumerable.Select(variableNames, v => (VariableName)v).ToArray());

            var trainingData = new TrainingData(sets, variables, TrainingDataSource.Csv);
            return trainingData;
        }

        public TrainingData LoadDefaultSetsFromFiles(string trainingSetFile, string validationSetFile, string testSetFile)
        {
            SupervisedTrainingSets sets;
            SupervisedSetVariables variables;

            if (string.IsNullOrWhiteSpace(trainingSetFile))
            {
                throw new ArgumentException("trainingSetFile must be non empty path to file");
            }
            else
            {
                var data = LoadDefaultSet(trainingSetFile);
                sets = new SupervisedTrainingSets(data.Sets.TrainingSet);
                variables = data.Variables;
            }


            if (!string.IsNullOrWhiteSpace(validationSetFile))
            {
                var data = LoadDefaultSet(validationSetFile);
                sets.ValidationSet = data.Sets.TrainingSet;
            }

            if (!string.IsNullOrWhiteSpace(testSetFile))
            {
                var data = LoadDefaultSet(testSetFile);
                sets.TestSet = data.Sets.TrainingSet;
            }

            if (sets.TrainingSet == null && sets.ValidationSet == null && sets.TestSet == null)
            {
                throw new ArgumentException("Invalid data set file names");
            }

            return new TrainingData(sets, variables, TrainingDataSource.Csv);
        }

        public TrainingData ChangeVariables(SupervisedSetVariableIndexes newVariableIndexes, TrainingData trainingData)
        {
            if (trainingData.Source == TrainingDataSource.Csv)
            {
                CsvFacade.ChangeVariableIndexes(newVariableIndexes, trainingData.Sets);
            }

            var newVariables = new SupervisedSetVariables(newVariableIndexes, trainingData.Variables.Names);
            return new TrainingData(trainingData.Sets, newVariables, trainingData.Source);
        }
    }
}