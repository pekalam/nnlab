using Common.Framework;
using NeuralNetwork.Application.Controllers;
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
        public NetDisplayViewModel(INetDisplayController controller, ModuleState moduleState)
        {
            Controller = controller;
            ModuleState = moduleState;
        }

        public INetDisplayController Controller { get; }

        public ModuleState ModuleState { get; }
    }
}
