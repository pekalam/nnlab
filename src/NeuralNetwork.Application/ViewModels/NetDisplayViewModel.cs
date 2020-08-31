using Common.Framework;
using NeuralNetwork.Application.Services;
using NNLibAdapter;
using Unity;

namespace NeuralNetwork.Application.ViewModels
{
    public class NetDisplayViewModel : ViewModelBase<NetDisplayViewModel>
    {
        
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public NetDisplayViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

            
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
