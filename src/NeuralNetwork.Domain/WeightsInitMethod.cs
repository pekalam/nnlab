using System;
using System.Linq;
using NNLib;
using NNLib.MLP;

namespace NeuralNetwork.Domain
{
    public enum WeightsInitMethod
    {
        SmallNumbers, NormalDist, Xavier, NguyenWidrow, SqrMUniform, SmallStdDev
    }

    public static class ParamsInitMethodExtentsions
    {
        public static WeightsInitMethod[] GetAvailableParamsInitMethods(this Layer layer, INetwork network)
        {
            return Enum.GetValues(typeof(WeightsInitMethod)).Cast<WeightsInitMethod>().Where(
                initMethod =>
                {
                    if (initMethod == WeightsInitMethod.NguyenWidrow &&
                        !(layer.IsOutputLayer || network.BaseLayers[0] == layer))
                    {
                        return false;
                    }

                    return true;
                }).ToArray();
        }
    }
}