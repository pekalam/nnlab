using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Domain;
using NNLib;
using NNLib.ActivationFunction;
using NNLib.MLP;
using NNLibAdapter;
using Prism.Ioc;

namespace NeuralNetwork.Domain
{
    public class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.Register<INeuralNetworkService, NeuralNetworkService>();
        }
    }
}
