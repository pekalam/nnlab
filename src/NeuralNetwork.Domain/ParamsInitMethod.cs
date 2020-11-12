using System;
using System.Linq;
using NNLib;
using NNLib.MLP;

namespace NeuralNetwork.Domain
{
    public enum ParamsInitMethod
    {
        DefaultNormalDist, NormalDist, Xavier, NguyenWidrow, SqrMUniform
    }

    public static class ParamsInitMethodExtentsions
    {
        public static ParamsInitMethod[] GetAvailableParamsInitMethods(this Layer layer, INetwork network)
        {
            return Enum.GetValues(typeof(ParamsInitMethod)).Cast<ParamsInitMethod>().Where(
                initMethod =>
                {
                    if (initMethod == ParamsInitMethod.NguyenWidrow &&
                        !(layer.IsOutputLayer || network.BaseLayers[0] == layer))
                    {
                        return false;
                    }

                    return true;
                }).ToArray();
        }
    }
}