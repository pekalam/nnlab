using System;
using System.Collections.Generic;
using NNLib.Common;
using NNLib.Csv;

namespace Infrastructure.Domain
{
    public enum TrainingDataSource
    {
        Memory,Csv
    }
    public class TrainingData
    {
        public SupervisedTrainingSets Sets { get; }
        public SupervisedSetVariables Variables { get; }
        public TrainingDataSource Source { get; }
        
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

        public SupervisedSet GetSet(DataSetType type)
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

        public TrainingData Clone()
        {
            SupervisedTrainingSets setsCpy = null;

            if (Source == TrainingDataSource.Csv)
            {
                setsCpy = CsvFacade.Copy(Sets);
            }
            else if (Source == TrainingDataSource.Memory)
            {
                throw new NotImplementedException();
            }


            return new TrainingData(setsCpy, Variables.Clone(), Source);
        }
    }
}