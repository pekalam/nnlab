using System;
using System.Collections.Generic;
using System.Linq;
using Common.Domain;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using NNLib;
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


        private Matrix<double> ToMinMax(Matrix<double> inputMat, DataSetType setType)
        {
            return Denormalization.ToMinMax(inputMat,
                _appState.ActiveSession!.TrainingData!.OriginalSets.ConcatenatedInput);
        }


        private Matrix<double> ToMean(Matrix<double> inputMat, DataSetType setType)
        {
            return Denormalization.ToMean(inputMat,
                _appState.ActiveSession!.TrainingData!.OriginalSets.ConcatenatedInput);
        }


        private Matrix<double> ToStd(Matrix<double> inputMat, DataSetType setType)
        {
            return Denormalization.ToStd(inputMat,
                _appState.ActiveSession!.TrainingData!.OriginalSets.ConcatenatedInput);
        }

        private Matrix<double> ToRobust(Matrix<double> inputMat, DataSetType setType)
        {
            return Denormalization.ToRobust(inputMat,
                _appState.ActiveSession!.TrainingData!.OriginalSets.ConcatenatedInput);
        }

        public Matrix<double> ToNetworkDataNormalization(Matrix<double> input, DataSetType setType)
        {
            return _appState.ActiveSession!.TrainingData!.NormalizationMethod
                switch
                {
                    NormalizationMethod.MinMax => ToMinMax(input, setType),
                    NormalizationMethod.Mean => ToMean(input,setType),
                    NormalizationMethod.Std => ToStd(input,setType),
                    NormalizationMethod.Robust => ToRobust(input, setType),
                    NormalizationMethod.None => input,
                    _ => throw new NotImplementedException(),
                };
        }
    }
}
