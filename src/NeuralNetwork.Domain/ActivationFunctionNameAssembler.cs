using System;
using NNLib.ActivationFunction;

namespace NeuralNetwork.Domain
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
}