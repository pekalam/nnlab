using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Messaging;
using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using NNLib;
using NNLibAdapter;
using Prism.Commands;
using Prism.Events;
using Shell.Interface;

namespace NeuralNetwork.Application.Controllers
{
    internal class LayersDisplayController : ControllerBase<LayersDisplayViewModel>,ITransientController<LayersDisplayService>
    {
        private LayersDisplayService _service = null!;
        private readonly INeuralNetworkShellService _shellService;
        private readonly INeuralNetworkService _networkService;
        private readonly AppState _appState;
        private readonly ModuleState _moduleState;
        private readonly IEventAggregator _ea;
        private readonly AppStateHelper _helper;

        public LayersDisplayController(INeuralNetworkShellService shellService, INeuralNetworkService networkService, AppState appState, IViewModelAccessor accessor, IEventAggregator ea, ModuleState moduleState) : base(accessor)
        {
            _shellService = shellService;
            _networkService = networkService;
            _appState = appState;
            _ea = ea;
            _moduleState = moduleState;
            _helper = new AppStateHelper(appState);
        }

        public void Initialize(LayersDisplayService service)
        {
            _service = service;
            service.AddLayerCommand = new DelegateCommand(AddLayer);
            service.RemoveLayerCommand = new DelegateCommand<LayerEditorItemModel>(RemoveLayer);
            service.EditLayerCommand = _shellService.OpenLayerEditorCommand;
            service.SelectLayerCommand = new DelegateCommand<Layer>(SelectLayer);
            service.LayerClickedCommand = new DelegateCommand<LayerEditorItemModel>(LayerClicked);
            service.InsertAfterCommand = new DelegateCommand<LayerEditorItemModel>(InsertAfter);
            service.InsertBeforeCommand = new DelegateCommand<LayerEditorItemModel>(InsertBefore);
        }

        protected override void VmCreated()
        {
            _helper.OnNetworkChanged(network =>
            {
                SetLayers();

                _moduleState.NetworkStructureChanged += ModuleStateOnNetworkStructureChanged;
            });
        }

        private void ModuleStateOnNetworkStructureChanged(NNLibModelAdapter obj)
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;

            if (obj.LayerModelAdapters[0].LayerModel.NeuronModels.Count != trainingData.Variables.InputVariableNames.Length ||
                obj.LayerModelAdapters[^1].LayerModel.NeuronModels.Count != trainingData.Variables.TargetVariableNames.Length) return;

            obj.SetInputLabels(trainingData.Variables.InputVariableNames);
            obj.SetOutputLabels(trainingData.Variables.TargetVariableNames);
        }

        private void InsertBefore(LayerEditorItemModel obj)
        {
            _networkService.InsertBefore(obj.LayerIndex);   
            SetLayers();
        }

        private void InsertAfter(LayerEditorItemModel obj)
        {
            if (obj.LayerIndex == _appState.ActiveSession!.Network!.TotalLayers - 1)
            {
                AddLayer();
            }
            else
            {
                _networkService.InsertAfter(obj.LayerIndex);
                SetLayers();
            }

        }

        private void LayerClicked(LayerEditorItemModel? obj)
        {
            if(obj == null || obj.IsAddLayerItem) return;
            //TODO fix
            _ea.GetEvent<IntLayerClicked>().Publish((_appState.ActiveSession!.Network!.Layers[obj.LayerIndex],obj.LayerIndex));
        }

        private void SelectLayer(Layer layer)
        {
            var neuralNetwork = _appState.ActiveSession!.Network!;
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

            var selected = Vm!.Layers.First(l =>
                l.LayerIndex == layerInd);

            Vm!.SelectedLayer = selected;
        }


        private void RemoveLayer(LayerEditorItemModel model)
        {
            var removed = _networkService.RemoveLayer(model.LayerIndex);
            if (removed.HasValue && !removed.Value)
            {
                PublishInvalidArch();
            }
            else
            {
                PublishValidArch();
            }
            //TODO remove
            SetLayers();
        }

        private void SetLayers()
        {
            Debug.Assert(_moduleState.ModelAdapter != null);
            
            _service.CreateLayers(_appState.ActiveSession!.Network!.Layers);
            _moduleState.ModelAdapter.Controller?.ClearHighlight();
        }

        private void AddLayer()
        {
            var neuralNetwork = _appState.ActiveSession!.Network!;
            if (!_networkService.AddLayer())
            {
                PublishInvalidArch();
            }
            else
            {
                PublishValidArch();
            }
            _service.AddLayer(neuralNetwork.BaseLayers[^1], neuralNetwork.TotalLayers - 1);
        }

        private void PublishInvalidArch()
        {
            _ea.GetEvent<ShowErrorNotification>().Publish(new ErrorNotificationArgs()
            {
                Message = "Invalid network architecture"
            });
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Training);
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Prediction);
        }

        private void PublishValidArch()
        {
            _ea.GetEvent<HideErrorNotification>().Publish();
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Training);
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Prediction);
        }
    }
}
