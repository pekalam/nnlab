using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Presentation.Views;
using Prism.Ioc;
using LayerEditorView = NeuralNetwork.Presentation.Views.LayerEditorView;
using LayersDisplayView = NeuralNetwork.Presentation.Views.LayersDisplayView;

namespace NeuralNetwork.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterForNavigation<NeuralNetworkShellView>();
            cr.RegisterForNavigation<LayerEditorView>();
            cr.RegisterForNavigation<LayersDisplayView>();
        }
    }
}