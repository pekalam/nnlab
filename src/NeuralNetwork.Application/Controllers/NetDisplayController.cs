using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Messaging;
using NNControl;
using NNLibAdapter;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;


namespace NeuralNetwork.Application.Controllers
{
    public interface INetDisplayController : ISingletonController
    {
        DelegateCommand<NeuronClickedEventArgs> NeuronClickCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<NetDisplayController>().RegisterSingleton<INetDisplayController, NetDisplayController>();
        }
    }


    internal class NetDisplayController : INetDisplayController
    {
        private readonly AppState _appState;
        private readonly AppStateHelper _helper;
        private readonly ModuleState _moduleState;
        private readonly IEventAggregator _ea;

        public NetDisplayController(ModuleState moduleState, IEventAggregator ea, AppState appState)
        {
            _ea = ea;
            _moduleState = moduleState;
            _appState = appState;
            _helper = new AppStateHelper(appState);

            NeuronClickCommand = new DelegateCommand<NeuronClickedEventArgs>(args =>
            {
                _ea.GetEvent<IntNeuronClicked>().Publish((((NNLibLayerAdapter)args.LayerAdapter).Layer, args.NeuronIndex)!);
            });

            ea.GetEvent<IntLayerClicked>().Subscribe(arg =>
            {
                _moduleState.ModelAdapter!.Controller.ClearHighlight();
                _moduleState.ModelAdapter.Controller.HighlightLayer(arg.layerIndex + 1);
            });

        }

        public void Initialize()
        {
        }

        public DelegateCommand<NeuronClickedEventArgs> NeuronClickCommand { get; set; }
    }
}
