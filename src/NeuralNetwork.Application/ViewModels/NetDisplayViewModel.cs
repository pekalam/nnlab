using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NNLibAdapter;
using Unity;

namespace NeuralNetwork.Application.ViewModels
{
    public class NetDisplayViewModel : ViewModelBase<NetDisplayViewModel>
    {
        private NNLibModelAdapter? _modelAdapter;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public NetDisplayViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

            
        }

        [InjectionConstructor]
        public NetDisplayViewModel(INetDisplayController controller)
        {
            Controller = controller;
            controller.Initialize(this);
        }

        public INetDisplayController Controller { get; }

        public NNLibModelAdapter? ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }
    }
}
