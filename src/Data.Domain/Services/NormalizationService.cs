using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Domain;
using NNLib.Common;
using NNLib.Csv;

namespace Data.Domain.Services
{
    public interface INormalizationDomainService
    {
        Task MinMaxNormalization(TrainingData trainingData);
        Task MeanNormalization(TrainingData trainingData);
        Task StdNormalization(TrainingData trainingData);
    }

    internal class NormalizationDomainService : INormalizationDomainService
    {
        public TrainingData Copy(TrainingData trainingData)
        {
            if (trainingData.Source == TrainingDataSource.Csv)
            {
                return new TrainingData(CsvFacade.Copy(trainingData.Sets), trainingData.Variables, trainingData.Source);
            }

            throw new NotImplementedException("Not implemented for in-memory source");
        }

        public Task MinMaxNormalization(TrainingData trainingData)
        {
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
                MinMax(trainingData.Sets.TrainingSet);
                if (trainingData.Sets.ValidationSet != null) MinMax(trainingData.Sets.ValidationSet);
                if (trainingData.Sets.TestSet != null) MinMax(trainingData.Sets.TestSet);
            });
        }

        public Task MeanNormalization(TrainingData trainingData)
        {
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
                Mean(trainingData.Sets.TrainingSet);
                if (trainingData.Sets.ValidationSet != null) Mean(trainingData.Sets.ValidationSet);
                if (trainingData.Sets.TestSet != null) Mean(trainingData.Sets.TestSet);
            });
        }

        public Task StdNormalization(TrainingData trainingData)
        {
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
                            stddev += Math.Pow(vec[j][i, 0] - avg, 2.0d) / vec.Count - 1;
                        }

                        stddev = Math.Sqrt(stddev);

                        for (int j = 0; j < vec.Count; j++)
                        {
                            vec[j][i, 0] = (vec[j][i, 0] - avg) / stddev;
                        }
                    }
                }
                StdVec(set.Input);
                StdVec(set.Target);
            }

            return Task.Run(() =>
            {
                Std(trainingData.Sets.TrainingSet);
                if (trainingData.Sets.ValidationSet != null) Std(trainingData.Sets.ValidationSet);
                if (trainingData.Sets.TestSet != null) Std(trainingData.Sets.TestSet);
            });
        }
    }
}
