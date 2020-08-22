using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Services;
using NNLib;
using Prism.Mvvm;
using Prism.Regions;
using SharedUI.MatrixPreview;
using Unity;

namespace NeuralNetwork.Application.ViewModels
{
    public class LayerDetailsModel : BindableBase
    {
        private ActivationFunctionName _activationFunction;
        private int _neuronsCount;
        private int _inputsCount;
        private ParamsInitMethod _paramsInitMethod = ParamsInitMethod.NormalDist;


        public LayerDetailsModel(Layer layer)
        {
            Layer = layer;
        }

        public Layer Layer { get; }

        public ActivationFunctionName ActivationFunction
        {
            get => _activationFunction;
            set => SetProperty(ref _activationFunction, value);
        }

        public int NeuronsCount
        {
            get => _neuronsCount;
            set => SetProperty(ref _neuronsCount, value);
        }

        public int InputsCount
        {
            get => _inputsCount;
            set => SetProperty(ref _inputsCount, value);
        }

        public ParamsInitMethod ParamsInitMethod
        {
            get => _paramsInitMethod;
            set => SetProperty(ref _paramsInitMethod, value);
        }

        public bool ShowInputsCount => Layer.IsInputLayer;
    }

    public class LayerEditorNavParams : NavigationParameters
    {
        public LayerEditorNavParams(MLPNetwork network, Layer layer, int layerNum)
        {
            Add(nameof(Layer), layer);
            Add(nameof(LayerNum), layerNum);
            Add(nameof(Network), network);
        }

        public int LayerNum => (int) this[nameof(LayerNum)];
        public Layer Layer => this[nameof(Layer)] as Layer;
        public MLPNetwork Network => this[nameof(Network)] as MLPNetwork;
    }

    public class LayerEditorViewModel : ViewModelBase<LayerEditorViewModel>
    {
        private LayerDetailsModel _layer;

        public LayerEditorViewModel()
        {
        }

        [InjectionConstructor]
        public LayerEditorViewModel(ILayerEditorService service)
        {
            Service = service;
        }

        public ILayerEditorService Service { get; }

        public MatrixPreviewViewModel MatrixPreview { get; set; } = new MatrixPreviewViewModel();

        public LayerDetailsModel Layer
        {
            get => _layer;
            set => SetProperty(ref _layer, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext.Parameters["params"] as LayerEditorNavParams);
        }
    }
}
