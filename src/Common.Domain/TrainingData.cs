using System;
using System.Collections.Generic;
using NNLib.Common;
using NNLib.Csv;
using Prism.Mvvm;

namespace Common.Domain
{
    public enum TrainingDataSource
    {
        Memory,Csv
    }
    public class TrainingData : BindableBase
    {
        private SupervisedSetVariables _variables;
        public SupervisedTrainingSets Sets { get; }
        public TrainingDataSource Source { get; }

        public SupervisedSetVariables Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value ?? throw new NullReferenceException("Null Variables"));
        }

        public TrainingData(SupervisedTrainingSets sets, SupervisedSetVariables variables, TrainingDataSource source)
        {
            Sets = sets;
            Variables = variables;
            Source = source;
        }

        public DataSetType[] SetTypes
        {
            get
            {
                List<DataSetType> setTypes = new List<DataSetType>();

                if (Sets.TrainingSet != null)
                {
                    setTypes.Add(DataSetType.Training);
                }

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

        public SupervisedSet? GetSet(DataSetType type)
        {
            switch (type)
            {
                case DataSetType.Test: return Sets.TestSet;
                case DataSetType.Training: return Sets.TrainingSet;
                case DataSetType.Validation: return Sets.ValidationSet;
                default: return Sets.TrainingSet;
            }
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

        private SupervisedSet CloneMemorySet(SupervisedSet set)
        {
            return new SupervisedSet((set.Input as DefaultVectorSet).Clone(),
                (set.Target as DefaultVectorSet).Clone());
        }

        public TrainingData Clone()
        {
            SupervisedTrainingSets setsCpy = null;

            if (Source == TrainingDataSource.Csv)
            {
                setsCpy = CsvFacade.Copy(Sets);
            }
            else if (Source == TrainingDataSource.Memory)
            {
                var training = CloneMemorySet(Sets.TrainingSet);

                setsCpy = new SupervisedTrainingSets(training)
                {
                    TestSet = Sets.TestSet != null ? CloneMemorySet(Sets.TestSet) : null,
                    ValidationSet = Sets.ValidationSet != null ? CloneMemorySet(Sets.ValidationSet) : null,
                };
            }


            return new TrainingData(setsCpy!, Variables.Clone(), Source);
        }
    }
}