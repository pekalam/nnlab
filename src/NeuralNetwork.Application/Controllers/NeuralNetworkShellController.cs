using System;
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
using Prism.Ioc;
using Prism.Regions;

namespace NeuralNetwork.Application.Services
{
    public interface INeuralNetworkShellService : IService
    {
        DelegateCommand<LayerEditorItemModel> OpenLayerEditorCommand { get; set; }
        DelegateCommand CloseLayerEditorCommand { get; set; }

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

        private bool _isEditorOpened;

        public NeuralNetworkShellController(IRegionManager rm, AppState appState)
        {
            _rm = rm;
            _appState = appState;

            OpenLayerEditorCommand = new DelegateCommand<LayerEditorItemModel>(OpenLayerEditor);
            CloseLayerEditorCommand = new DelegateCommand(CloseLayerEditor);

            NeuralNetworkShellViewModel.Created += () =>
            {
                NeuralNetworkShellViewModel.Instance.Navigated += () =>
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
            _isEditorOpened = true;
            var layer = _appState.ActiveSession.Network.Layers[model.LayerIndex];
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
    }
}
