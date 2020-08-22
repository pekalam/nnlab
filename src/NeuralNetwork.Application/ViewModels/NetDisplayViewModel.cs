using Common.Framework;
using NeuralNetwork.Application.Services;
using NNLibAdapter;

namespace NeuralNetwork.Application.ViewModels
{
    public class NetDisplayViewModel : ViewModelBase<NetDisplayViewModel>
    {
        private NNLibModelAdapter _modelAdapter;

        public NetDisplayViewModel(INetDisplayService service)
        {
            Service = service;
        }

        public INetDisplayService Service { get; }

        public NNLibModelAdapter ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }
    }
}
