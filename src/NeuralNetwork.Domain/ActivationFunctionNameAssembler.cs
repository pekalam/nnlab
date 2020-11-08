using NNLib.ActivationFunction;
using NNLib.MLP;
using System;

namespace Common.Domain
{
    public static class ActivationFunctionNameAssembler
    {
        public static ActivationFunctionName FromActivationFunction(IActivationFunction activationFunction)
        {
            switch (activationFunction)
            {
                case SigmoidActivationFunction _:
                    return ActivationFunctionName.Sigmoid;
                case LinearActivationFunction _:
                    return ActivationFunctionName.Linear;
                case TanHActivationFunction _:
                    return ActivationFunctionName.TanH;
                case ArcTanActivationFunction _:
                    return ActivationFunctionName.ArcTan;
                default:
                    throw new ArgumentException("Invalid activation function");
            }
        }

        public static IActivationFunction FromActivationFunctionName(ActivationFunctionName name)
        {
            switch (name)
            {
                case ActivationFunctionName.Linear:
                    return new LinearActivationFunction();
                case ActivationFunctionName.Sigmoid:
                    return new SigmoidActivationFunction();
                case ActivationFunctionName.ArcTan:
                    return new ArcTanActivationFunction();
                case ActivationFunctionName.TanH:
                    return new TanHActivationFunction();
                default:
                    throw new ArgumentException("Invalid activation function name");
            }
        }
    }


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