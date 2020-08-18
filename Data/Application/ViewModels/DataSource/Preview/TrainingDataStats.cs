﻿using System;
using Infrastructure.Domain;
using NNLib.Common;

namespace Data.Application.ViewModels.DataSource.Preview
{
    public class TrainingDataStats
    {
        private readonly TrainingData _trainingData;

        public TrainingDataStats(TrainingData trainingData)
        {
            _trainingData = trainingData;
        }

        public int GetRowsForSet(DataSetType setType)
        {
            if (setType == DataSetType.Training)
            {
                return _trainingData.Sets.TrainingSet.Input.Count;
            }

            if (setType == DataSetType.Test)
            {
                return _trainingData.Sets.TestSet.Input.Count;
            }

            if (setType == DataSetType.Validation)
            {
                return _trainingData.Sets.ValidationSet.Input.Count;
            }

            throw new ArgumentException();
        }

        public int GetColumnsForSet(DataSetType setType)
        {
            return _trainingData.Variables.Names.Length;
        }
    }
}