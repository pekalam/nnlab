﻿using System;
using System.Collections.Generic;
using NNLib.Common;
using NNLib.Csv;
using NNLib.Data;
using Prism.Mvvm;

namespace Common.Domain
{
    public enum TrainingDataSource
    {
        Memory,Csv
    }

    public enum NormalizationMethod
    {
        None,MinMax,Mean,Std
    }

    public class TrainingData : BindableBase
    {
        private SupervisedTrainingSamplesVariables _variables;
        private NormalizationMethod _normalizationMethod;
        private SupervisedTrainingData _sets;
        public TrainingDataSource Source { get; }

        public SupervisedTrainingData Sets
        {
            get => _sets;
            set => SetProperty(ref _sets, value);
        }

        public SupervisedTrainingData OriginalSets { get; }

        public NormalizationMethod NormalizationMethod
        {
            get => _normalizationMethod;
            set => SetProperty(ref _normalizationMethod, value);
        }

        public SupervisedTrainingSamplesVariables Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value ?? throw new NullReferenceException("Null Variables"));
        }

        public TrainingData(SupervisedTrainingData sets, SupervisedTrainingSamplesVariables variables, TrainingDataSource source, NormalizationMethod normalizationMethod)
        {
            _sets = sets;
            _variables = variables;
            Source = source;
            _normalizationMethod = normalizationMethod;
            OriginalSets = CloneSets();
        }

        public DataSetType[] SetTypes
        {
            get
            {
                List<DataSetType> setTypes = new List<DataSetType>();

                setTypes.Add(DataSetType.Training);

                if (Sets.ValidationSet != null)
                {
                    setTypes.Add(DataSetType.Validation);
                }

                if (Sets.TestSet != null)
                {
                    setTypes.Add(DataSetType.Test);
                }

                return setTypes.ToArray();
            }
        }

        public SupervisedTrainingSamples? GetSet(DataSetType type)
        {
            switch (type)
            {
                case DataSetType.Test: return Sets.TestSet;
                case DataSetType.Training: return Sets.TrainingSet;
                case DataSetType.Validation: return Sets.ValidationSet;
                default: return null;
            }
        }

        public void RestoreOriginalSets()
        {
            Sets = OriginalSets;
            NormalizationMethod = NormalizationMethod.None;
        }


        public bool IsValid
        {
            get
            {
                if (Variables.Indexes.InputVarIndexes.Length > 0 && Variables.Indexes.TargetVarIndexes.Length > 0)
                {
                    return true;
                }

                return false;
            }
        }

        private SupervisedTrainingSamples CloneMemorySet(SupervisedTrainingSamples set)
        {
            return new SupervisedTrainingSamples((set.Input as DefaultVectorSet)!.Clone(),
                (set.Target as DefaultVectorSet)!.Clone());
        }

        public TrainingData Clone()
        {
            SupervisedTrainingData setsCpy = CloneSets();

            return new TrainingData(setsCpy!, Variables.Clone(), Source, NormalizationMethod);
        }


        public SupervisedTrainingData CloneSets()
        {
            SupervisedTrainingData? setsCpy = null;

            if (Source == TrainingDataSource.Csv)
            {
                setsCpy = CsvFacade.Copy(Sets);
            }
            else if (Source == TrainingDataSource.Memory)
            {
                var training = CloneMemorySet(Sets.TrainingSet);

                setsCpy = new SupervisedTrainingData(training)
                {
                    TestSet = Sets.TestSet != null ? CloneMemorySet(Sets.TestSet) : null,
                    ValidationSet = Sets.ValidationSet != null ? CloneMemorySet(Sets.ValidationSet) : null,
                };
            }else
            {
                throw new NotImplementedException();
            }

            return setsCpy;
        }
    }
}