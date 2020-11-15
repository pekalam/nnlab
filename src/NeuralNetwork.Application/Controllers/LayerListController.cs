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
using System.ComponentModel;
using System.Linq;

namespace NeuralNetwork.Application.Controllers
{
    public interface ILayerListController : IController
    {
        DelegateCommand AddLayerCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> RemoveLayerCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> EditLayerCommand { get; set; }
        DelegateCommand<Layer> SelectLayerCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> LayerClickedCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> InsertAfterCommand { get; set; }
        DelegateCommand<LayerEditorItemModel> InsertBeforeCommand { get; set; }

        Action<int> NavigatedFromOpened { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ILayerListController, LayerListController>();
        }
    }

    internal class LayerListController : ControllerBase<LayerListViewModel>,ILayerListController
    {
        private SubscriptionToken _nClickSub;
        private SubscriptionToken _areaClickSub;

        private readonly INeuralNetworkShellController _shellService;
        private readonly INeuralNetworkService _networkService;
        private readonly AppState _appState;
        private readonly IEventAggregator _ea;
        private readonly AppStateHelper _helper;

        private bool _initialized;

        public LayerListController(INeuralNetworkShellController shellService, INeuralNetworkService networkService, AppState appState, IEventAggregator ea)
        {
            _shellService = shellService;
            _networkService = networkService;
            _appState = appState;
            _ea = ea;
            _helper = new AppStateHelper(appState);

            AddLayerCommand = new DelegateCommand(AddLayer);
            RemoveLayerCommand = new DelegateCommand<LayerEditorItemModel>(RemoveLayer);
            EditLayerCommand = _shellService.OpenLayerEditorCommand;
            SelectLayerCommand = new DelegateCommand<Layer>(SelectLayer);
            LayerClickedCommand = new DelegateCommand<LayerEditorItemModel>(LayerClicked);
            InsertAfterCommand = new DelegateCommand<LayerEditorItemModel>(InsertAfter);
            InsertBeforeCommand = new DelegateCommand<LayerEditorItemModel>(InsertBefore);

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
                SetLayers();
            });

            _nClickSub = _ea.GetEvent<IntNeuronClicked>().Subscribe(args =>
            {
                SelectLayer(args);
            });

            _areaClickSub = _ea.GetEvent<IntNetDisplayAreaClicked>().Subscribe(() =>
            {
                Vm!.SelectedLayer = null;
            });

            Vm!.IsActiveChanged += (sender, args) =>
            {
                if (!Vm.IsActive)
                {
                    _areaClickSub.Dispose();
                    _nClickSub.Dispose();
                }
            };

            _initialized = true;
        }

        private void InsertBefore(LayerEditorItemModel obj)
        {
            _networkService.InsertBefore(obj.LayerIndex);   
            SetLayers();
            _ea.GetEvent<IntLayerListChanged>().Publish();
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
            _ea.GetEvent<IntLayerListChanged>().Publish();
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
            _ea.GetEvent<IntLayerListChanged>().Publish();
        }

        private void SetLayers() => Vm!.CreateLayers(_appState.ActiveSession!.Network!.Layers);

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
            SetLayers();
            _ea.GetEvent<IntLayerListChanged>().Publish();
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

        public DelegateCommand AddLayerCommand { get; set; }
        public DelegateCommand<LayerEditorItemModel> RemoveLayerCommand { get; set; }
        public DelegateCommand<LayerEditorItemModel> EditLayerCommand { get; set; }
        public DelegateCommand<Layer> SelectLayerCommand { get; set; }
        public DelegateCommand<LayerEditorItemModel> LayerClickedCommand { get; set; }
        public DelegateCommand<LayerEditorItemModel> InsertAfterCommand { get; set; }
        public DelegateCommand<LayerEditorItemModel> InsertBeforeCommand { get; set; }
        public Action<int> NavigatedFromOpened { get; }
    }
}
