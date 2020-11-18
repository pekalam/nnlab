using NNLib.Csv;
using NNLib.Data;
using Prism.Mvvm;
using System;
using System.Collections.Generic;

namespace Common.Domain
{
    public enum TrainingDataSource
    {
        Memory,Csv
    }

    public enum NormalizationMethod
    {
        None,MinMax,Mean,Std,Robust,
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
            private set => SetProperty(ref _sets, value);
        }

        public SupervisedTrainingData OriginalSets { get; private set; }

        public NormalizationMethod NormalizationMethod
        {
            get => _normalizationMethod;
            private set => SetProperty(ref _normalizationMethod, value);
        }

        public SupervisedTrainingSamplesVariables Variables
        {
            get => _variables;
            private set => SetProperty(ref _variables, value ?? throw new NullReferenceException("Null Variables"));
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
        
        public SupervisedTrainingSamples? GetOriginalSet(DataSetType type)
        {
            switch (type)
            {
                case DataSetType.Test: return OriginalSets.TestSet;
                case DataSetType.Training: return OriginalSets.TrainingSet;
                case DataSetType.Validation: return OriginalSets.ValidationSet;
                default: return null;
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

        public SupervisedTrainingData CloneOriginalSets() => InternalCloneSets(OriginalSets);

        public SupervisedTrainingData CloneSets() => InternalCloneSets(Sets);

        private SupervisedTrainingData InternalCloneSets(SupervisedTrainingData toCpy)
        {
            SupervisedTrainingData? setsCpy = null;

            if (Source == TrainingDataSource.Csv)
            {
                setsCpy = CsvFacade.Copy(toCpy);
            }
            else if (Source == TrainingDataSource.Memory)
            {
                var training = CloneMemorySet(toCpy.TrainingSet);

                setsCpy = new SupervisedTrainingData(training)
                {
                    TestSet = toCpy.TestSet != null ? CloneMemorySet(toCpy.TestSet) : null,
                    ValidationSet = toCpy.ValidationSet != null ? CloneMemorySet(toCpy.ValidationSet) : null,
                };
            }else
            {
                throw new NotImplementedException();
            }

            return setsCpy;
        }

        public void StoreNewSets(SupervisedTrainingData sets)
        {
            _normalizationMethod = NormalizationMethod.None;
            OriginalSets = InternalCloneSets(sets);
            Sets = sets;
        }

        public void ChangeNormalization(SupervisedTrainingData newData, NormalizationMethod newNormalization)
        {
            _sets = newData;
            NormalizationMethod = newNormalization;
        }

        public void ChangeVariables(SupervisedTrainingSamplesVariables variables)
        {
            Variables = variables;
        }
    }
}