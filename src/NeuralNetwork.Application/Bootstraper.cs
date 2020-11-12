using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Domain;
using Prism.Ioc;

namespace NeuralNetwork.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ModuleState>();

            cr.Register<INeuralNetworkService, NNControlNeuralNetworkServiceDecorator>();

            INetDisplayController.Register(cr);
            ILayerEditorController.Register(cr);
            ILayerListController.Register(cr);
            INeuralNetworkShellController.Register(cr);
        }
    }
}