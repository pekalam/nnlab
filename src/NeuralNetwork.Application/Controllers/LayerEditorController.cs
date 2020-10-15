using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Common.Domain;
using Common.Framework;
using ControlzEx.Standard;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using NNLib;
using NNLib.MLP;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using SharedUI.MatrixPreview;
using Shell.Interface;

namespace NeuralNetwork.Application.Services
{
    public interface ILayerEditorService : IService
    {
        Action<LayerEditorNavParams> Navigated { get; set; }
        DelegateCommand ExitCommand { get; set; }
        DelegateCommand InitializeParametersCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ILayerEditorService, LayerEditorController>();
        }
    }
}

namespace NeuralNetwork.Application.Controllers
{
    internal class LayerEditorController : ControllerBase<LayerEditorViewModel>,ITransientController, ILayerEditorService
    {
        private readonly INeuralNetworkService _networkService;
        private readonly IEventAggregator _ea;

        private MLPNetwork? _assignedNetwork;
        private int _layerNum;

        public LayerEditorController(INeuralNetworkShellService shellService, IViewModelAccessor accessor, INeuralNetworkService networkService, IEventAggregator ea) : base(accessor)
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
            var model = new LayerDetailsModel(layer)
            {
                NeuronsCount = layer.NeuronsCount,
                InputsCount = layer.InputsCount,
                ActivationFunction =
                    ActivationFunctionNameAssembler.FromActivationFunction(
                        ((PerceptronLayer)layer).ActivationFunction),
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
                    _networkService.ChangeParamsInitMethod(Vm!.Layer!.ParamsInitMethod);
                    Vm!.MatrixPreview.Controller.AssignNetwork(_assignedNetwork!);
                    Vm!.MatrixPreview.Controller.SelectMatrix(_layerNum, MatrixTypes.Weights);
                    break;
            }
        }

        private void InitializeParameters()
        {
            _networkService.ResetWeights(_assignedNetwork!.Layers[_layerNum]);
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
