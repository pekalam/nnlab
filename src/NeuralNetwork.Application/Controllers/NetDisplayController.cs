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
            _helper.OnNetworkChanged(network =>
            {
                var adapter = new NNLibModelAdapter(network);

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
                adapter.AttachInputLabels(data.Variables.InputVariableNames);
            }
        }
        private void SetOutputLabels(NNLibModelAdapter adapter, TrainingData data)
        {
            if (data.Variables.TargetVariableNames.Length == _appState.ActiveSession!.Network!.Layers[^1].NeuronsCount)
            {
                adapter.AttachOutputLabels(data.Variables.TargetVariableNames);
            }
        }

        private void SetNetworkLabels(NNLibModelAdapter adapter,TrainingData data)
        {
            SetInputLabels(adapter, data);
            SetOutputLabels(adapter, data);
        }



        public DelegateCommand<int?> NeuronClickCommand { get; set; } = null!;
        public DelegateCommand AreaClicked { get; set; } = null!;
    }
}
