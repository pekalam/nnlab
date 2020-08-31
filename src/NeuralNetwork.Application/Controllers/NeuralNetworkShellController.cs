﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Ribbon;
using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.View;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;

namespace NeuralNetwork.Application.Services
{
    public interface INeuralNetworkShellService : IService
    {
        DelegateCommand<LayerEditorItemModel> OpenLayerEditorCommand { get; set; }
        DelegateCommand CloseLayerEditorCommand { get; set; }
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<NeuralNetworkShellController>().RegisterSingleton<INeuralNetworkShellService, NeuralNetworkShellController>();
        }
    }
}

namespace NeuralNetwork.Application.Controllers
{
    internal class NeuralNetworkShellController : ISingletonController, INeuralNetworkShellService
    {
        private readonly IRegionManager _rm;
        private readonly AppState _appState;
        private readonly IEventAggregator _ea;

        private bool _isEditorOpened;

        public NeuralNetworkShellController(IRegionManager rm, AppState appState, IEventAggregator ea)
        {
            _rm = rm;
            _appState = appState;
            _ea = ea;

            OpenLayerEditorCommand = new DelegateCommand<LayerEditorItemModel>(OpenLayerEditor);
            CloseLayerEditorCommand = new DelegateCommand(CloseLayerEditor);


            Navigated = (_) =>
            {
                _rm.Regions[NeuralNetworkRegions.NetworkDownRegion].RequestNavigate(nameof(LayersDisplayViewModel));
            };

            _appState.ActiveSessionChanged += (_, __) =>
            {
                if (_isEditorOpened) CloseLayerEditor();
            };
        }

        private void CloseLayerEditor()
        {
            _isEditorOpened = false;
            if (_rm.Regions.ContainsRegionWithName(NeuralNetworkRegions.NetworkDownRegion))
            {
                _rm.Regions[NeuralNetworkRegions.NetworkDownRegion].RequestNavigate(nameof(LayersDisplayViewModel));
            }
        }

        private void OpenLayerEditor(LayerEditorItemModel model)
        {
            _ea.GetEvent<EnableModalNavigation>().Publish(CloseLayerEditorCommand);
            _isEditorOpened = true;
            var layer = _appState.ActiveSession!.Network!.Layers[model.LayerIndex];
            _rm.Regions[NeuralNetworkRegions.NetworkDownRegion].RequestNavigate(nameof(LayerEditorViewModel),new NavigationParameters()
            {
                {"params", new LayerEditorNavParams(_appState.ActiveSession.Network, layer, model.LayerIndex)}
            });
        }


        public void Initialize()
        {
        }

        public DelegateCommand<LayerEditorItemModel> OpenLayerEditorCommand { get; set; }
        public DelegateCommand CloseLayerEditorCommand { get; set; }
        public Action<NavigationContext> Navigated { get; private set; }
    }
}
