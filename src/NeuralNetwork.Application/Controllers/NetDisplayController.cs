using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.Messaging;
using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.ViewModels;
using NNControl;
using NNLibAdapter;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;


namespace NeuralNetwork.Application.Services
{
    public interface INetDisplayService : IService
    {
        DelegateCommand<NeuronClickedEventArgs> NeuronClickCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<NetDisplayController>().RegisterSingleton<INetDisplayService, NetDisplayController>();
        }
    }
}

namespace NeuralNetwork.Application.Controllers
{
    internal class NetDisplayController : ISingletonController, INetDisplayService
    {
        private readonly ModuleState _moduleState;
        private readonly IEventAggregator _ea;

        public NetDisplayController(ModuleState moduleState, IEventAggregator ea)
        {
            _moduleState = moduleState;
            _ea = ea;

            _moduleState.PropertyChanged += (sender, args) =>
            {
                if (NetDisplayViewModel.Instance != null && args.PropertyName == nameof(ModuleState.ModelAdapter))
                {
                    NetDisplayViewModel.Instance.ModelAdapter = _moduleState.ModelAdapter;
                }
            };

            NeuronClickCommand = new DelegateCommand<NeuronClickedEventArgs>(args =>
            {
                _ea.GetEvent<IntNeuronClicked>().Publish((((NNLibLayerAdapter)args.LayerAdapter).Layer, args.NeuronIndex));
            });


            NetDisplayViewModel.Created += () =>
                {
                    NetDisplayViewModel.Instance.ModelAdapter = _moduleState.ModelAdapter;
                };

            _ea.GetEvent<IntLayerClicked>().Subscribe(arg =>
            {
                _moduleState.ModelAdapter.Controller.ClearHighlight();
                _moduleState.ModelAdapter.Controller.HighlightLayer(arg.layerIndex + 1);
            });
        }

        public void Initialize()
        {
        }

        public DelegateCommand<NeuronClickedEventArgs> NeuronClickCommand { get; set; }
    }
}
