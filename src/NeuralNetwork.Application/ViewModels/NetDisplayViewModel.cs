using Common.Framework;
using NeuralNetwork.Application.Services;
using NNLibAdapter;
using Unity;

namespace NeuralNetwork.Application.ViewModels
{
    public class NetDisplayViewModel : ViewModelBase<NetDisplayViewModel>
    {
        public NetDisplayViewModel()
        {
            
        }

        [InjectionConstructor]
        public NetDisplayViewModel(INetDisplayService service, ModuleState moduleState)
        {
            Service = service;
            ModuleState = moduleState;
        }

        public INetDisplayService Service { get; }

        public ModuleState ModuleState { get; }
    }
}
