﻿using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Domain;
using Prism.Ioc;

namespace NeuralNetwork.Application
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            INetDisplayController.Register(cr);
            ILayerEditorController.Register(cr);
            ILayerListController.Register(cr);
            INeuralNetworkShellController.Register(cr);
        }
    }
}