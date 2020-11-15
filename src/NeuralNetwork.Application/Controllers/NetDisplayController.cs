using System.Windows;
using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Messaging;
using NeuralNetwork.Application.ViewModels;
using NNControl;
using NNLibAdapter;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;


namespace NeuralNetwork.Application.Controllers
{
    public interface INetDisplayController : IController
    {
        DelegateCommand<int?> NeuronClickCommand { get; set; }
        DelegateCommand AreaClicked { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<INetDisplayController, NetDisplayController>();
        }
    }


    internal class NetDisplayController : ControllerBase<NetDisplayViewModel>,INetDisplayController
    {
        private readonly AppState _appState;
        private readonly AppStateHelper _helper;
        private readonly IEventAggregator _ea;
        private readonly INeuralNetworkShellController _shellController;

        public NetDisplayController(IEventAggregator ea, AppState appState, INeuralNetworkShellController shellController)
        {
            _ea = ea;
            _appState = appState;
            _shellController = shellController;
            _helper = new AppStateHelper(appState);
        }

        protected override void VmCreated()
        {
            _ea.GetEvent<IntLayerAdded>().Subscribe(arg =>
            {
                if (Vm!.ModelAdapter!.Layers[^1] == arg.layer)
                {
                    Vm!.ModelAdapter.AddLayer(arg.layer);
                    SetOutputLabels(Vm!.ModelAdapter, _appState.ActiveSession!.TrainingData!);
                    return;
                }


                for (int i = 0; i < Vm!.ModelAdapter.Layers.Count; i++)
                {
                    if (Vm!.ModelAdapter.Layers[i] == arg.layer && (i == arg.index || arg.index == 0))
                    {
                        Vm!.ModelAdapter.InsertBefore(arg.index + 1, arg.layer);
                        return;
                    }
                    if (Vm!.ModelAdapter.Layers[i] == arg.layer && i + 1 == arg.index)
                    {
                        Vm!.ModelAdapter.InsertAfter(arg.index + 1, arg.layer);
                        return;
                    }
                }
            });

            _ea.GetEvent<IntLayerRemoved>().Subscribe(index =>
            {
                Vm!.ModelAdapter!.RemoveLayer(index);
                if (index == _appState.ActiveSession!.Network!.Layers.Count)
                {
                    SetOutputLabels(Vm!.ModelAdapter, _appState.ActiveSession!.TrainingData!);
                }
                if (index == 0)
                {
                    Vm!.ModelAdapter.LayerModelAdapters[0].SetNeuronsCount(_appState.ActiveSession!.Network!.Layers[0].InputsCount);
                }
            });


            _ea.GetEvent<IntLayerModified>().Subscribe(arg =>
            {
                Vm!.ModelAdapter!.LayerModelAdapters[arg.index].SetNeuronsCount(arg.neuronsCount);

                if (arg.index == 0)
                {
                    SetInputLabels(Vm!.ModelAdapter, _appState.ActiveSession!.TrainingData!);
                }
                else if (arg.index == _appState.ActiveSession!.Network!.TotalLayers)
                {
                    SetOutputLabels(Vm!.ModelAdapter, _appState.ActiveSession!.TrainingData!);
                }
            });

            _helper.OnNetworkChanged(network =>
            {
                var adapter = new NNLibModelAdapter();
                adapter.SetNeuralNetwork(network);

                if (_appState.ActiveSession!.TrainingData != null)
                {
                    SetNetworkLabels(adapter,_appState.ActiveSession!.TrainingData);
                }
                adapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";


                Vm!.ModelAdapter = adapter;
            });

            _helper.OnTrainingDataPropertyChanged(data => SetNetworkLabels(Vm!.ModelAdapter!, data), s => s == nameof(TrainingData.Variables));

            NeuronClickCommand = new DelegateCommand<int?>(layerInd =>
            {
                if (!layerInd.HasValue || layerInd == 0)
                {
                    return;
                }
                layerInd--;

                _ea.GetEvent<IntNeuronClicked>().Publish(_appState.ActiveSession!.Network!.Layers[layerInd.Value]);
            });

            _ea.GetEvent<IntLayerClicked>().Subscribe(arg =>
            {
                Vm!.ModelAdapter!.Controller.ClearHighlight();
                Vm!.ModelAdapter!.Controller.HighlightLayer(arg.layerIndex + 1);
            });

            _ea.GetEvent<IntLayerListChanged>().Subscribe(() =>
            {
                Vm!.ModelAdapter!.Controller.ClearHighlight();
            });

            AreaClicked = new DelegateCommand(() =>
            {
                if(_shellController.IsEditorOpened) return;

                Vm!.ModelAdapter!.Controller.ClearHighlight();
                _ea.GetEvent<IntNetDisplayAreaClicked>().Publish();
            });
        }


        private void SetInputLabels(NNLibModelAdapter adapter, TrainingData data)
        {
            if (data.Variables.InputVariableNames.Length == _appState.ActiveSession!.Network!.Layers[0].InputsCount)
            {
                adapter.SetInputLabels(data.Variables.InputVariableNames);
            }
        }
        private void SetOutputLabels(NNLibModelAdapter adapter, TrainingData data)
        {
            if (data.Variables.TargetVariableNames.Length == _appState.ActiveSession!.Network!.Layers[^1].NeuronsCount)
            {
                adapter.SetOutputLabels(data.Variables.TargetVariableNames);
            }
        }

        private void SetNetworkLabels(NNLibModelAdapter adapter,TrainingData data)
        {
            SetInputLabels(adapter, data);
            SetOutputLabels(adapter, data);
        }



        public DelegateCommand<int?> NeuronClickCommand { get; set; }
        public DelegateCommand AreaClicked { get; set; }
    }
}
