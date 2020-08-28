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

        public Task MinMaxNormalization()
        {
            if (_moduleState.OriginalTrainingData == null)
            {
                _moduleState.StoreOriginalTrainingData();
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

            return Task.Run(() =>
            {
                var trainingData = _appState.ActiveSession.TrainingData.Clone();
                MinMax(trainingData.Sets.TrainingSet);
                if (trainingData.Sets.ValidationSet != null) MinMax(trainingData.Sets.ValidationSet);
                if (trainingData.Sets.TestSet != null) MinMax(trainingData.Sets.TestSet);
                _appState.ActiveSession.TrainingData = trainingData;
            });
        }

        public Task MeanNormalization()
        {
            if (_moduleState.OriginalTrainingData == null)
            {
                _moduleState.StoreOriginalTrainingData();
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

            return Task.Run(() =>
            {
                var trainingData = _appState.ActiveSession.TrainingData.Clone();
                Mean(trainingData.Sets.TrainingSet);
                if (trainingData.Sets.ValidationSet != null) Mean(trainingData.Sets.ValidationSet);
                if (trainingData.Sets.TestSet != null) Mean(trainingData.Sets.TestSet);
                _appState.ActiveSession.TrainingData = trainingData;
            });
        }

        public Task StdNormalization()
        {
            if (_moduleState.OriginalTrainingData == null)
            {
                _moduleState.StoreOriginalTrainingData();
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

            return Task.Run(() =>
            {
                var trainingData = _appState.ActiveSession.TrainingData.Clone();
                Std(trainingData.Sets.TrainingSet);
                if (trainingData.Sets.ValidationSet != null) Std(trainingData.Sets.ValidationSet);
                if (trainingData.Sets.TestSet != null) Std(trainingData.Sets.TestSet);
                _appState.ActiveSession.TrainingData = trainingData;
            });
        }

        public void NoNormalization()
        {
            _appState.ActiveSession.TrainingData = _moduleState.OriginalTrainingData;
        }
    }
}
