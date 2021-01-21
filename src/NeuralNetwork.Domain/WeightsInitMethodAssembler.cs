using System;
using NNLib;
using NNLib.MLP;

namespace NeuralNetwork.Domain
{
    public static class WeightsInitMethodAssembler
    {
        public static WeightsInitMethod FromLayer(Layer layer)
        {
            return layer.MatrixBuilder switch
            {
                SqrMUniformMatrixBuilder _ => WeightsInitMethod.SqrMUniform,
                NguyenWidrowMatrixBuilder _ => WeightsInitMethod.NguyenWidrow,
                SmallNumbersMatrixBuilder _ => WeightsInitMethod.SmallNumbers,
                SmallStdevNormDistMatrixBuilder _ => WeightsInitMethod.SmallStdDev,
                NormDistMatrixBuilder _ => WeightsInitMethod.NormalDist,
                XavierMatrixBuilder _ => WeightsInitMethod.Xavier,
                _ => throw new NotImplementedException(),
            };
        }

        public static MatrixBuilder FromWeightsInitMethod<T>(WeightsInitMethod method, T? options = null) where T : class
        {
            return method switch
            {
                WeightsInitMethod.NguyenWidrow => new NguyenWidrowMatrixBuilder(),
                WeightsInitMethod.SqrMUniform => new SqrMUniformMatrixBuilder(),
                WeightsInitMethod.SmallNumbers => new SmallNumbersMatrixBuilder(),
                WeightsInitMethod.SmallStdDev => new SmallStdevNormDistMatrixBuilder(),
                WeightsInitMethod.NormalDist => new NormDistMatrixBuilder(options as NormDistMatrixBuilderOptions ??
                                                                         throw new ArgumentException("Invalid norm dist options type")),
                WeightsInitMethod.Xavier => new XavierMatrixBuilder(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}