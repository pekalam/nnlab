using Common.Domain;
using NNLib.Data;
using System;
using System.Threading.Tasks;

namespace Data.Domain.Services
{
    public interface INormalizationDomainService
    {
        Task MinMaxNormalization();
        Task MeanNormalization();
        Task StdNormalization();
        void NoNormalization();

        Task Normalize(NormalizationMethod method);
    }

    internal class NormalizationDomainService : INormalizationDomainService
    {
        private AppState _appState;

        public NormalizationDomainService(AppState appState)
        {
            _appState = appState;
        }

        public async Task MinMaxNormalization()
        {
            void MinMax(SupervisedTrainingSamples set)
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
            }

            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingData? sets = null;
            await Task.Run(() =>
            {
                sets = trainingData.CloneSets(true);
                MinMax(sets.TrainingSet);
                if (sets.ValidationSet != null) MinMax(sets.ValidationSet);
                if (sets.TestSet != null) MinMax(sets.TestSet);

            });
            trainingData.ChangeNormalization(sets!, NormalizationMethod.MinMax);
        }

        public async Task MeanNormalization()
        {
            void Mean(SupervisedTrainingSamples set)
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
            }


            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingData? sets = null;
            await Task.Run(() =>
            {
                sets = trainingData.CloneSets(true);
                Mean(sets.TrainingSet);
                if (sets.ValidationSet != null) Mean(sets.ValidationSet);
                if (sets.TestSet != null) Mean(sets.TestSet);
            });
            trainingData.ChangeNormalization(sets!, NormalizationMethod.Mean);
        }

        public async Task StdNormalization()
        {
            void Std(SupervisedTrainingSamples set)
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
            }

            var trainingData = _appState.ActiveSession!.TrainingData!;
            SupervisedTrainingData? sets = null;
            await Task.Run(() =>
            {
                sets = trainingData.CloneSets(true);
                Std(sets.TrainingSet);
                if (sets.ValidationSet != null) Std(sets.ValidationSet);
                if (sets.TestSet != null) Std(sets.TestSet);
            });
            trainingData.ChangeNormalization(sets!, NormalizationMethod.Std);
        }

        public void NoNormalization()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            trainingData.ChangeNormalization(trainingData.OriginalSets, NormalizationMethod.None);
        }

        public async Task Normalize(NormalizationMethod method)
        {
            switch (method)
            {
                case NormalizationMethod.None:
                    NoNormalization();
                    break;
                case NormalizationMethod.Mean:
                    await MeanNormalization();
                    break;
                case NormalizationMethod.MinMax:
                    await MinMaxNormalization();
                    break;
                case NormalizationMethod.Std:
                    await StdNormalization();
                    break;
            }
        }
    }
}
