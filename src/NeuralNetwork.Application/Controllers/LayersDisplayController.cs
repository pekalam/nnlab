using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Messaging;
using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using NNLib;
using Prism.Commands;
using Prism.Events;

namespace NeuralNetwork.Application.Controllers
{
    internal class LayersDisplayController : ITransientController<LayersDisplayService>
    {
        private LayersDisplayService _service;
        private readonly INeuralNetworkShellService _shellService;
        private readonly INeuralNetworkService _networkService;
        private readonly AppState _appState;
        private readonly IViewModelAccessor _accessor;
        private readonly IEventAggregator _ea;

        public LayersDisplayController(INeuralNetworkShellService shellService, INeuralNetworkService networkService, AppState appState, IViewModelAccessor accessor, IEventAggregator ea)
        {
            _shellService = shellService;
            _networkService = networkService;
            _appState = appState;
            _accessor = accessor;
            _ea = ea;
        }

        public void Initialize(LayersDisplayService service)
        {
            _service = service;
            service.AddLayerCommand = new DelegateCommand(AddLayer);
            service.RemoveLayerCommand = new DelegateCommand<LayerEditorItemModel>(RemoveLayer);
            service.EditLayerCommand = _shellService.OpenLayerEditorCommand;
            service.SelectLayerCommand = new DelegateCommand<Layer>(SelectLayer);
            service.LayerClickedCommand = new DelegateCommand<LayerEditorItemModel>(LayerClicked);

            _appState.PropertyChanged += AppStateOnPropertyChanged;

            _accessor.OnCreated<LayersDisplayViewModel>(() =>
            {
                SetLayers();
            });

        }

        private void LayerClicked(LayerEditorItemModel obj)
        {
            if(obj == null) return;
            //TODO fix
            _ea.GetEvent<IntLayerClicked>().Publish((_appState.ActiveSession.Network.Layers[obj.LayerIndex],obj.LayerIndex));
        }

        private void SelectLayer(Layer layer)
        {
            var neuralNetwork = _appState.ActiveSession.Network;
            var vm = _accessor.Get<LayersDisplayViewModel>();
            int layerInd = -1;
            for (int i = 0; i < neuralNetwork.TotalLayers; i++)
            {
                if (neuralNetwork.BaseLayers[i] == layer)
                {
                    layerInd = i;
                    break;
                }
            }

            if (layerInd == -1)
            {
                throw new Exception();
            }

            var selected = vm.Layers.First(l =>
                l.LayerIndex == layerInd);

            vm.SelectedLayer = selected;
        }


        private void AppStateOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppState.ActiveSession))
            {
                if(_accessor.Get<LayersDisplayViewModel>() != null && _appState.ActiveSession.Network != null) SetLayers();
            }
        }


        private void RemoveLayer(LayerEditorItemModel model)
        {
            _networkService.RemoveLayer(model.LayerIndex);
            //TODO remove
            SetLayers();
        }

        private void SetLayers()
        {
            var neuralNetwork = _appState.ActiveSession.Network;
            _service.CreateLayers(neuralNetwork.Layers);
        }

        private void AddLayer()
        {
            var neuralNetwork = _appState.ActiveSession.Network;
            _networkService.AddLayer();
            _service.AddLayer(neuralNetwork.BaseLayers[^1], neuralNetwork.TotalLayers - 1);
        }
    }
}
