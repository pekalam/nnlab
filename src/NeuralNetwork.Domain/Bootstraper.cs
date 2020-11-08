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
