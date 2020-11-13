using System;
using Common.Domain;
using MathNet.Numerics.LinearAlgebra;
using NNLib.Data;

namespace Approximation.Application
{
    internal class NormalizationService
    {
        private AppState _appState;

        public NormalizationService(AppState appState)
        {
            _appState = appState;
        }

        private (double min, double max) FindMinMax(IVectorSet vectorSet, int row)
        {
            double min = double.MaxValue;
            double max = double.MinValue;

            foreach (var mat in vectorSet)
            {
                if (mat.At(row, 0) < min) min = mat.At(row, 0);
                if (mat.At(row, 0) > max) max = mat.At(row, 0);
            }

            return (min, max);
        }

        private Matrix<double> ToMinMax(Matrix<double> inputMat)
        {
            var input = _appState.ActiveSession!.TrainingData!.OriginalSets.TrainingSet.Input;
            var y = inputMat.Clone();

            for (int i = 0; i < inputMat.RowCount; i++)
            {
                var (min, max) = FindMinMax(input, i);

                y[i, 0] = (y[i, 0] - min) / (max - min);
            }

            return y;
        }


        private (double avg, double min, double max) FindMean(IVectorSet vec, int row)
        {
            double avg = 0;
            double min = double.MaxValue;
            double max = double.MinValue;
            foreach (var mat in vec)
            {
                if (mat[row, 0] < min)
                {
                    min = mat[row, 0];
                }
                if (mat[row, 0] > max)
                {
                    max = mat[row, 0];
                }

                avg += mat[row, 0] / vec.Count;
            }

            return (avg, min, max);
        }


        private Matrix<double> ToMean(Matrix<double> inputMat)
        {
            var trainingInput = _appState.ActiveSession!.TrainingData!.OriginalSets.TrainingSet.Input;
            var y = inputMat.Clone();

            for (int i = 0; i < inputMat.RowCount; i++)
            {
                var (avg, min, max) = FindMean(trainingInput, i);

                y[i, 0] = (y[i, 0] - avg) / (max - min);
            }

            return y;
        }


        private Matrix<double> ToStd(Matrix<double> inputMat)
        {
            var trainingInput = _appState.ActiveSession!.TrainingData!.OriginalSets.TrainingSet.Input;
            var y = inputMat.Clone();

            double stddev = 0;
            for (int i = 0; i < inputMat.RowCount; i++)
            {
                var (avg, min, max) = FindMean(trainingInput, i);

                for (int j = 0; j < trainingInput.Count; j++)
                {
                    stddev += Math.Pow(trainingInput[j][i, 0] - avg, 2.0d) / (trainingInput.Count - 1);
                }
                stddev = Math.Sqrt(stddev);

                y[i,0] = (y[i, 0] - avg) / (stddev == 0d ? 1 : stddev);
            }

            return y;
        }

        public Matrix<double> ToNetworkDataNormalization(Matrix<double> input)
        {
            return _appState.ActiveSession!.TrainingData!.NormalizationMethod
                switch
                {
                    NormalizationMethod.MinMax => ToMinMax(input),
                    NormalizationMethod.Mean => ToMean(input),
                    NormalizationMethod.Std => ToStd(input),
                    NormalizationMethod.None => input,
                    _ => throw new Exception(),
                };
        }
    }
}
