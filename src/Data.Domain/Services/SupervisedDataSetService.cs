using Common.Domain;
using NNLib.Csv;
using NNLib.Data;
using System;
using System.Linq;

namespace Data.Domain.Services
{
    public interface ITrainingDataService
    {
        TrainingData LoadDefaultTrainingData(string fileName, IDataSetDivider? divider =null, DataSetDivisionOptions? divisionOptions=null, SupervisedSetVariableIndexes? variableIndexes=null);
        SupervisedTrainingData LoadSets(string fileName, IDataSetDivider? divider, DataSetDivisionOptions? divisionOptions, SupervisedSetVariableIndexes? variableIndexes);
        TrainingData LoadDefaultTrainingDataFromFiles(string trainingSetFile, string validationSetFile, string testSetFile);
        void ChangeVariables(SupervisedSetVariableIndexes newVariableIndexes, TrainingData trainingData);
        void ChangeVariables(SupervisedSetVariableIndexes newVariableIndexes, SupervisedTrainingData sets, TrainingDataSource source);
    }

    internal class TrainingDataService : ITrainingDataService
    {
        public TrainingData LoadDefaultTrainingData(string fileName, IDataSetDivider? divider = null,
            DataSetDivisionOptions? divisionOptions = null, SupervisedSetVariableIndexes? indices = null)
        {
            var (sets,variableNames, variableIndexes) = CsvFacade.LoadSets(fileName, divider, divisionOptions, indices);
            var variables = new SupervisedTrainingSamplesVariables(variableIndexes, variableNames.Select(v => (VariableName)v).ToArray());

            return new TrainingData(sets,variables, TrainingDataSource.Csv, NormalizationMethod.None);
        }

        public SupervisedTrainingData LoadSets(string fileName, IDataSetDivider? divider,
            DataSetDivisionOptions? divisionOptions, SupervisedSetVariableIndexes? variableIndexes)
        {
            var (sets, _, __) = CsvFacade.LoadSets(fileName, divider, divisionOptions, variableIndexes);
            return sets;
        }

        public TrainingData LoadDefaultTrainingDataFromFiles(string trainingSetFile, string validationSetFile, string testSetFile)
        {
            SupervisedTrainingData sets;
            SupervisedTrainingSamplesVariables variables;

            if (string.IsNullOrWhiteSpace(trainingSetFile))
            {
                throw new ArgumentException("trainingSetFile must be non empty path to file");
            }
            else
            {
                var data = LoadDefaultTrainingData(trainingSetFile);
                sets = new SupervisedTrainingData(data.Sets.TrainingSet);
                variables = data.Variables;
            }


            if (!string.IsNullOrWhiteSpace(validationSetFile))
            {
                var data = LoadDefaultTrainingData(validationSetFile);
                sets.ValidationSet = data.Sets.TrainingSet;
            }

            if (!string.IsNullOrWhiteSpace(testSetFile))
            {
                var data = LoadDefaultTrainingData(testSetFile);
                sets.TestSet = data.Sets.TrainingSet;
            }

            if (sets.TrainingSet == null && sets.ValidationSet == null && sets.TestSet == null)
            {
                throw new ArgumentException("Invalid data set file names");
            }

            return new TrainingData(sets, variables, TrainingDataSource.Csv, NormalizationMethod.None);
        }

        public void ChangeVariables(SupervisedSetVariableIndexes newVariableIndexes, TrainingData trainingData)
        {
            if (trainingData.Source == TrainingDataSource.Csv)
            {
                CsvFacade.ChangeVariableIndexes(newVariableIndexes, trainingData.Sets);
            }

            var newVariables = new SupervisedTrainingSamplesVariables(newVariableIndexes, trainingData.Variables.Names);
            trainingData.ChangeVariables(newVariables);
        }

        public void ChangeVariables(SupervisedSetVariableIndexes newVariableIndexes, SupervisedTrainingData sets, TrainingDataSource source)
        {
            if (source == TrainingDataSource.Csv)
            {
                CsvFacade.ChangeVariableIndexes(newVariableIndexes, sets);
            }
        }
    }
}