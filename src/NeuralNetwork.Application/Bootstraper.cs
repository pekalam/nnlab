using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.Services;
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

            INetDisplayService.Register(cr);
            ILayerEditorService.Register(cr);
            ILayersDisplayController.Register(cr);
            INeuralNetworkShellService.Register(cr);
        }
    }
}