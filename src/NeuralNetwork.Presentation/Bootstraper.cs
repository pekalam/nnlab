using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Presentation.Views;
using Prism.Ioc;

namespace NeuralNetwork.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterForNavigation<ViewA>(nameof(ViewAViewModel));
        }
    }
}