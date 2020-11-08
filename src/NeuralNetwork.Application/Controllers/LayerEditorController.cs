using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using NNLib.MLP;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using SharedUI.MatrixPreview;
using Shell.Interface;
using System;
using System.ComponentModel;

namespace NeuralNetwork.Application.Controllers
{
    public interface ILayerEditorController : ITransientController
    {
        Action<LayerEditorNavParams> Navigated { get; set; }
        DelegateCommand ExitCommand { get; set; }
        DelegateCommand InitializeParametersCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ILayerEditorController, LayerEditorController>();
        }
    }

    internal class LayerEditorController : ControllerBase<LayerEditorViewModel>,ILayerEditorController
    {
        private readonly INeuralNetworkService _networkService;
        private readonly IEventAggregator _ea;

        private MLPNetwork? _assignedNetwork;
        private int _layerNum;

        public LayerEditorController(INeuralNetworkShellController shellService, INeuralNetworkService networkService, IEventAggregator ea)
        {
            _networkService = networkService;
            _ea = ea;

            Navigated = OnNavigated;
            ExitCommand = shellService.CloseLayerEditorCommand;
            InitializeParametersCommand = new DelegateCommand(InitializeParameters);
        }

        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand InitializeParametersCommand { get; set; }
        public Action<LayerEditorNavParams> Navigated { get; set; }

        private void OnNavigated(LayerEditorNavParams navParams)
        {
            var layer = navParams.Layer;
            _layerNum = navParams.LayerNum;
            _assignedNetwork = navParams.Network;
            var model = new LayerDetailsModel(layer, navParams.Network.BaseLayers[0] == layer)
            {
                NeuronsCount = layer.NeuronsCount,
                InputsCount = layer.InputsCount,
                ActivationFunction =
                    ActivationFunctionNameAssembler.FromActivationFunction(
                        ((PerceptronLayer)layer).ActivationFunction),
                ParamsInitMethod = ParamsInitMethodAssembler.FromMatrixBuilder(layer.MatrixBuilder),
            };
            model.PropertyChanged += OnLayerDetailsModelPropertyChanged;

            Vm!.Layer = model;

            Vm!.MatrixPreview.Controller.AssignNetwork(navParams.Network);
            Vm!.MatrixPreview.Controller.SelectMatrix(navParams.LayerNum, MatrixTypes.Weights);
        }

        private void OnLayerDetailsModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = (LayerDetailsModel)sender;

            switch (e.PropertyName)
            {
                case nameof(model.ActivationFunction):
                    _networkService.SetActivationFunction(model.Layer,
                        ActivationFunctionNameAssembler.FromActivationFunctionName(model.ActivationFunction));
                    break;
                case nameof(model.NeuronsCount):
                    if (!_networkService.SetNeuronsCount(model.Layer, model.NeuronsCount))
                    {
                        PublishInvalidArch();
                    }
                    else
                    {
                        PublishValidArch();
                    }

                    Vm!.MatrixPreview.Controller.InvalidateDisplayedMatrix();
                    break;
                case nameof(model.InputsCount):
                    if (!_networkService.SetInputsCount(model.InputsCount))
                    {
                        PublishInvalidArch();
                    }
                    else
                    {
                        PublishValidArch();
                    }
                    Vm!.MatrixPreview.Controller.InvalidateDisplayedMatrix();
                    break;
                case nameof(model.ParamsInitMethod):
                    _networkService.ChangeParamsInitMethod(Vm!.Layer!.Layer, Vm!.Layer!.ParamsInitMethod, false, Vm!.Layer.ParamsInitMethod == ParamsInitMethod.NormalDist ? Vm!.Layer.NormDistOptions : null);
                    break;
            }
        }

        private void InitializeParameters()
        {
            _networkService.ChangeParamsInitMethod(Vm!.Layer!.Layer,Vm!.Layer!.ParamsInitMethod, true, Vm!.Layer.ParamsInitMethod == ParamsInitMethod.NormalDist ? Vm!.Layer.NormDistOptions : null);
            Vm!.MatrixPreview.Controller.AssignNetwork(_assignedNetwork!);
            Vm!.MatrixPreview.Controller.SelectMatrix(_layerNum, MatrixTypes.Weights);
        }

        private void PublishInvalidArch()
        {
            _ea.GetEvent<ShowErrorNotification>().Publish(new ErrorNotificationArgs()
            {
                Message = "Invalid network architecture"
            });
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Training);
            _ea.GetEvent<DisableNavMenuItem>().Publish(ModuleIds.Prediction);
        }

        private void PublishValidArch()
        {
            _ea.GetEvent<HideErrorNotification>().Publish();
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Data);
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Training);
            _ea.GetEvent<EnableNavMenuItem>().Publish(ModuleIds.Prediction);
        }
    }
}
