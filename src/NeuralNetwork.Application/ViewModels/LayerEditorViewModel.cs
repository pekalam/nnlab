using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NNLib;
using NNLib.MLP;
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
        private NormDistMatrixBuilderOptions _normDistOptions = new NormDistMatrixBuilderOptions()
        {
            WMean = 0, WStdDev = 1, BMean = 0, BStdDev = 0,
        };


        public LayerDetailsModel(Layer layer, bool isFirstLayer)
        {
            Layer = layer;
            ShowInputsCount = isFirstLayer;
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

        public NormDistMatrixBuilderOptions NormDistOptions
        {
            get => _normDistOptions;
            set => SetProperty(ref _normDistOptions, value);
        }

        public bool ShowInputsCount { get; }
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
        public Layer Layer => (this[nameof(Layer)] as Layer)!;
        public MLPNetwork Network => (this[nameof(Network)] as MLPNetwork)!;
    }

    public class LayerEditorViewModel : ViewModelBase<LayerEditorViewModel>
    {
        private LayerDetailsModel? _layer;

        
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public LayerEditorViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        [InjectionConstructor]
        public LayerEditorViewModel(ILayerEditorController service)
        {
            Service = service;
            service.Initialize(this);
        }

        public ILayerEditorController Service { get; }

        public MatrixPreviewViewModel MatrixPreview { get; set; } = new MatrixPreviewViewModel();

        public LayerDetailsModel? Layer
        {
            get => _layer;
            set => SetProperty(ref _layer, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated((LayerEditorNavParams)navigationContext.Parameters["params"]);
        }
    }
}
