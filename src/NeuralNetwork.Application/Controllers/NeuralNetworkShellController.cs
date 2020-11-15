using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.View;
using NeuralNetwork.Application.ViewModels;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;
using System;
using Unity.Injection;

namespace NeuralNetwork.Application.Controllers
{
    public interface INeuralNetworkShellController : ISingletonController
    {
        DelegateCommand<LayerEditorItemModel> OpenLayerEditorCommand { get; set; }
        DelegateCommand CloseLayerEditorCommand { get; set; }
        Action<NavigationContext> Navigated { get; }

        bool IsEditorOpened { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<NeuralNetworkShellController>().RegisterSingleton<INeuralNetworkShellController, NeuralNetworkShellController>();
        }
    }
    internal class NeuralNetworkShellController : INeuralNetworkShellController
    {
        private readonly IRegionManager _rm;
        private readonly AppState _appState;
        private readonly IEventAggregator _ea;
        private LayerEditorItemModel? _openedModel;

        public NeuralNetworkShellController(IRegionManager rm, AppState appState, IEventAggregator ea)
        {
            _rm = rm;
            _appState = appState;
            _ea = ea;

            OpenLayerEditorCommand = new DelegateCommand<LayerEditorItemModel>(OpenLayerEditor);
            CloseLayerEditorCommand = new DelegateCommand(CloseLayerEditor);


            Navigated = (_) =>
            {
                _rm.Regions[NeuralNetworkRegions.NetworkDownRegion].RequestNavigate("LayerListView");
            };

            _appState.ActiveSessionChanged += (_, __) =>
            {
                if (IsEditorOpened) CloseLayerEditor();
            };
        }

        private void CloseLayerEditor()
        {
            IsEditorOpened = false;
            if (_rm.Regions.ContainsRegionWithName(NeuralNetworkRegions.NetworkDownRegion))
            {
                _rm.Regions[NeuralNetworkRegions.NetworkDownRegion].RequestNavigate("LayerListView", new NavigationParameters
                {
                    {"PreviousSelected", _openedModel!.LayerIndex}
                });
            }

            _openedModel = null;
        }

        private void OpenLayerEditor(LayerEditorItemModel model)
        {
            _openedModel = model;
            _ea.GetEvent<EnableModalNavigation>().Publish(CloseLayerEditorCommand);
            IsEditorOpened = true;
            var layer = _appState.ActiveSession!.Network!.Layers[model.LayerIndex];
            _rm.Regions[NeuralNetworkRegions.NetworkDownRegion].RequestNavigate("LayerEditorView", new NavigationParameters()
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
        public bool IsEditorOpened { get; private set; }
    }
}
