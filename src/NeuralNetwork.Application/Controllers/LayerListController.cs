using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Messaging;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using NNLib;
using NNLibAdapter;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Shell.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace NeuralNetwork.Application.Controllers
{
    public interface ILayerListController : IController
    {
        DelegateCommand<LayerListItemModel> LayerClickedCommand { get; set; }

        Action<int> NavigatedFromOpened { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ILayerListController, LayerListController>();
        }
    }

    internal class LayerListController : ControllerBase<LayerListViewModel>,ILayerListController
    {
        private SubscriptionToken _nClickSub = null!;
        private SubscriptionToken _areaClickSub = null!;

        private readonly INeuralNetworkService _networkService;
        private readonly AppState _appState;
        private readonly IEventAggregator _ea;
        private readonly AppStateHelper _helper;

        private bool _initialized;

        public LayerListController(INeuralNetworkShellController shellController, INeuralNetworkService networkService, AppState appState, IEventAggregator ea)
        {
            _networkService = networkService;
            _appState = appState;
            _ea = ea;
            _helper = new AppStateHelper(appState);

            AddLayerCommand = new DelegateCommand(AddLayer);
            RemoveLayerCommand = new DelegateCommand<LayerListItemModel>(RemoveLayer);
            EditLayerCommand = shellController.OpenLayerEditorCommand;
            LayerClickedCommand = new DelegateCommand<LayerListItemModel>(LayerClicked);
            InsertAfterCommand = new DelegateCommand<LayerListItemModel>(InsertAfter);
            InsertBeforeCommand = new DelegateCommand<LayerListItemModel>(InsertBefore);

            NavigatedFromOpened = layerIndex =>
            {
                Vm!.SelectedLayer = Vm!.Layers[layerIndex];
            };
        }

        protected override void VmCreated()
        {
            if (_initialized) return;

            _helper.OnNetworkChanged(network =>
            {
                CreateLayers();
            });

            _nClickSub = _ea.GetEvent<IntNeuronClicked>().Subscribe(SelectLayer);

            _areaClickSub = _ea.GetEvent<IntNetDisplayAreaClicked>().Subscribe(() =>
            {
                Vm!.SelectedLayer = null;
            });

            Vm!.IsActiveChanged += (sender, args) =>
            {
                if (!Vm!.IsActive)
                {
                    _areaClickSub.Dispose();
                    _nClickSub.Dispose();
                }
            };

            _initialized = true;
        }

        private void LayerClicked(LayerListItemModel? obj)
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

            Debug.Assert(layerInd != -1);

            var selected = Vm!.Layers.First(l =>
                l.LayerIndex == layerInd);

            Vm!.SelectedLayer = selected;
        }

        private void InsertBefore(LayerListItemModel model)
        {
            if (_networkService.InsertBefore(model.LayerIndex))
            {
                PublishValidArch();
            }
            else
            {
                PublishInvalidArch();
            }
            CreateLayers();
            _ea.GetEvent<IntLayerListChanged>().Publish();
        }

        private void InsertAfter(LayerListItemModel obj)
        {
            if (_networkService.InsertAfter(obj.LayerIndex))
            {
                PublishValidArch();
            }
            else
            {
                PublishInvalidArch();
            }
            CreateLayers();
            _ea.GetEvent<IntLayerListChanged>().Publish();
        }


        private void RemoveLayer(LayerListItemModel model)
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

            CreateLayers();
            _ea.GetEvent<IntLayerListChanged>().Publish();
        }
        private void AddLayer()
        {
            if (!_networkService.AddLayer())
            {
                PublishInvalidArch();
            }
            else
            {
                PublishValidArch();
            }
            CreateLayers();
            _ea.GetEvent<IntLayerListChanged>().Publish();
        }

        private void CreateLayers()
        {
            var collection = new ObservableCollection<LayerListItemModel>(
                _appState.ActiveSession!.Network!.Layers.Select(CreateLayerModel)
                );
            collection.Add(new LayerListItemModel()
            {
                IsAddLayerItem = true,
                IsOutputLayer = false,
                LayerIndex = collection.Count,
                AddLayer = AddLayerCommand
            });
            Vm!.Layers = collection;
        }

        private LayerListItemModel CreateLayerModel(Layer layer, int ind)
        {
            return new LayerListItemModel()
            {
                IsFirstLayer = ind == 0,
                IsOutputLayer = layer.IsOutputLayer,
                LayerIndex = ind,
                RemoveLayer = RemoveLayerCommand,
                EditLayer = EditLayerCommand,
                InsertAfter = InsertAfterCommand,
                InsertBefore = InsertBeforeCommand
            };
        }

        private void PublishInvalidArch()
        {
            _ea.GetEvent<ShowErrorNotification>().Publish(new ErrorNotificationArgs()
            {
                Message = "Invalid network architecture"
            });
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Training);
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Approximation);
        }

        private void PublishValidArch()
        {
            _ea.GetEvent<HideErrorNotification>().Publish();
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Training);
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Approximation);
        }

        private DelegateCommand AddLayerCommand { get; }
        private DelegateCommand<LayerListItemModel> RemoveLayerCommand { get; }
        private DelegateCommand<LayerListItemModel> EditLayerCommand { get; }
        private DelegateCommand<LayerListItemModel> InsertAfterCommand { get; }
        private DelegateCommand<LayerListItemModel> InsertBeforeCommand { get; }
        public DelegateCommand<LayerListItemModel> LayerClickedCommand { get; set; }
        public Action<int> NavigatedFromOpened { get; }
    }
}
