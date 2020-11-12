using System;
using NNLib.MLP;

namespace NeuralNetwork.Domain
{
    public static class ParamsInitMethodAssembler
    {
        public static ParamsInitMethod FromMatrixBuilder(MatrixBuilder matrixBuilder)
        {
            return matrixBuilder switch
            {
                SqrMUniformMatrixBuilder _ => ParamsInitMethod.SqrMUniform,
                NguyenWidrowMatrixBuilder _ => ParamsInitMethod.NguyenWidrow,
                DefaultNormDistMatrixBuilder _ => ParamsInitMethod.DefaultNormalDist,
                NormDistMatrixBuilder _ => ParamsInitMethod.NormalDist,
                XavierMatrixBuilder _ => ParamsInitMethod.Xavier,
                _ => throw new NotImplementedException(),
            };
        }

        public static MatrixBuilder FromParamsInitMethod<T>(ParamsInitMethod method, T? options = null) where T : class
        {
            return method switch
            {
                ParamsInitMethod.NguyenWidrow => new NguyenWidrowMatrixBuilder(),
                ParamsInitMethod.SqrMUniform => new SqrMUniformMatrixBuilder(),
                ParamsInitMethod.DefaultNormalDist => new DefaultNormDistMatrixBuilder(),
                ParamsInitMethod.NormalDist => new NormDistMatrixBuilder(options as NormDistMatrixBuilderOptions ??
                                                                         throw new ArgumentException("Invalid norm dist options type")),
                ParamsInitMethod.Xavier => new XavierMatrixBuilder(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}