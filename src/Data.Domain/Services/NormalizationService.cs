using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common.Domain;
using NNLib.Common;
using NNLib.Csv;

namespace Data.Domain.Services
{
    public interface INormalizationDomainService
    {
        Task MinMaxNormalization();
        Task MeanNormalization();
        Task StdNormalization();
        void NoNormalization();
    }

    internal class NormalizationDomainService : INormalizationDomainService
    {
        private ModuleState _moduleState;
        private AppState _appState;

        public NormalizationDomainService(ModuleState moduleState, AppState appState)
        {
            _moduleState = moduleState;
            _appState = appState;
        }

        public async Task MinMaxNormalization()
        {
            if (_moduleState.OriginalSets == null)
            {
                _moduleState.StoreOriginalSets();
            }


            void MinMax(SupervisedSet set)
            {
                void MinMaxVec(IVectorSet vec)
                {
                    for (int i = 0; i < vec[0].RowCount; i++)
                    {
                        double min, max;
                        min = max = vec[0][i, 0];
                        for (int j = 0; j < vec.Count; j++)
                        {
                            if (vec[j][i, 0] < min)
                            {
                                min = vec[j][i, 0];
                            }
                            if (vec[j][i, 0] > max)
                            {
                                max = vec[j][i, 0];
                            }
                        }


                        for (int j = 0; j < vec.Count; j++)
                        {
                            if (max == min)
                            {
                                vec[j][i, 0] = 0;
                            }
                            else
                            {
                                vec[j][i, 0] = (vec[j][i, 0] - min) / (max - min);
                            }
                        }
                    }
                }
                MinMaxVec(set.Input);
                MinMaxVec(set.Target);
            }

            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingSets? sets = null;
            await Task.Run(() =>
            {
                sets = trainingData.CloneSets();
                MinMax(sets.TrainingSet);
                if (sets.ValidationSet != null) MinMax(sets.ValidationSet);
                if (sets.TestSet != null) MinMax(sets.TestSet);

            });
            trainingData.NormalizationMethod = NormalizationMethod.MinMax;
            trainingData.Sets = sets!;
        }

        public async Task MeanNormalization()
        {
            if (_moduleState.OriginalSets == null)
            {
                _moduleState.StoreOriginalSets();
            }

            void Mean(SupervisedSet set)
            {
                void MeanVec(IVectorSet vec)
                {
                    for (int i = 0; i < vec[0].RowCount; i++)
                    {
                        double min, max, avg = 0;
                        min = max = vec[0][i, 0];
                        for (int j = 0; j < vec.Count; j++)
                        {
                            if (vec[j][i, 0] < min)
                            {
                                min = vec[j][i, 0];
                            }
                            if (vec[j][i, 0] > max)
                            {
                                max = vec[j][i, 0];
                            }

                            avg += vec[j][i, 0] / vec.Count;
                        }


                        for (int j = 0; j < vec.Count; j++)
                        {
                            if (max == min)
                            {
                                vec[j][i, 0] = 0;
                            }
                            else
                            {
                                vec[j][i, 0] = (vec[j][i, 0] - avg) / (max - min);
                            }
                        }
                    }
                }
                MeanVec(set.Input);
                MeanVec(set.Target);
            }


            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingSets? sets = null;
            await Task.Run(() =>
            {
                sets = trainingData.CloneSets();
                Mean(sets.TrainingSet);
                if (sets.ValidationSet != null) Mean(sets.ValidationSet);
                if (sets.TestSet != null) Mean(sets.TestSet);
            });
            trainingData.NormalizationMethod = NormalizationMethod.Mean;
            trainingData.Sets = sets!;
        }

        public async Task StdNormalization()
        {
            if (_moduleState.OriginalSets == null)
            {
                _moduleState.StoreOriginalSets();
            }

            void Std(SupervisedSet set)
            {
                void StdVec(IVectorSet vec)
                {
                    for (int i = 0; i < vec[0].RowCount; i++)
                    {
                        double min, max, avg = 0, stddev = 0;
                        min = max = vec[0][i, 0];
                        for (int j = 0; j < vec.Count; j++)
                        {
                            if (vec[j][i, 0] < min)
                            {
                                min = vec[j][i, 0];
                            }
                            if (vec[j][i, 0] > max)
                            {
                                max = vec[j][i, 0];
                            }

                            avg += vec[j][i, 0] / vec.Count;
                        }

                        for (int j = 0; j < vec.Count; j++)
                        {
                            stddev += Math.Pow(vec[j][i, 0] - avg, 2.0d) / (vec.Count - 1);
                        }

                        stddev = Math.Sqrt(stddev);

                        for (int j = 0; j < vec.Count; j++)
                        {
                            vec[j][i, 0] = (vec[j][i, 0] - avg) / (stddev == 0d ? 1 : stddev);
                        }
                    }
                }
                StdVec(set.Input);
                StdVec(set.Target);
            }

            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingSets? sets = null;
            await Task.Run(() =>
            {
                sets = trainingData.CloneSets();
                Std(sets.TrainingSet);
                if (sets.ValidationSet != null) Std(sets.ValidationSet);
                if (sets.TestSet != null) Std(sets.TestSet);
            });
            trainingData.NormalizationMethod = NormalizationMethod.Std;
            trainingData.Sets = sets!;
        }

        public void NoNormalization()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            trainingData.Sets = _moduleState.OriginalSets ?? throw new NullReferenceException("No original sets stored");
            trainingData.NormalizationMethod = NormalizationMethod.None;
        }
    }
}
