﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Common.Domain;
using Common.Framework;
using NeuralNetwork.Application.Controllers;
using NeuralNetwork.Application.Services;
using NeuralNetwork.Application.ViewModels;
using NeuralNetwork.Domain;
using NNLib;
using Prism.Commands;
using Prism.Ioc;
using SharedUI.MatrixPreview;

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
    internal class LayerEditorController : ITransientController, ILayerEditorService
    {
        private INeuralNetworkShellService _shellService;
        private readonly IViewModelAccessor _accessor;
        private readonly INeuralNetworkService _networkService;
        private int _layerNum;

        public LayerEditorController(INeuralNetworkShellService shellService, IViewModelAccessor accessor, INeuralNetworkService networkService)
        {
            _shellService = shellService;
            _accessor = accessor;
            _networkService = networkService;
            Navigated = OnNavigated;
            ExitCommand = shellService.CloseLayerEditorCommand;
        }

        public DelegateCommand ExitCommand { get; set; }
        public DelegateCommand InitializeParametersCommand { get; set; }
        public Action<LayerEditorNavParams> Navigated { get; set; }


        private void OnNavigated(LayerEditorNavParams navParams)
        {
            var vm = _accessor.Get<LayerEditorViewModel>();
            var layer = navParams.Layer;
            _layerNum = navParams.LayerNum;
            var model = new LayerDetailsModel(layer)
            {
                NeuronsCount = layer.NeuronsCount,
                InputsCount = layer.InputsCount,
                ActivationFunction =
                    ActivationFunctionNameAssembler.FromActivationFunction(
                        ((PerceptronLayer)layer).ActivationFunction),
            };
            model.PropertyChanged += OnLayerDetailsModelPropertyChanged;

            vm.Layer = model;

            vm.MatrixPreview.Controller.AssignNetwork(navParams.Network);
            vm.MatrixPreview.Controller.SelectMatrix(navParams.LayerNum, MatrixTypes.Weights);
        }

        private void OnLayerDetailsModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = _accessor.Get<LayerEditorViewModel>();
            var model = (LayerDetailsModel)sender;

            if (e.PropertyName == nameof(model.ActivationFunction))
            {
                _networkService.SetActivationFunction(model.Layer,
                    ActivationFunctionNameAssembler.FromActivationFunctionName(model.ActivationFunction));
            }
            else if (e.PropertyName == nameof(model.NeuronsCount))
            {
                _networkService.SetNeuronsCount(model.Layer, model.NeuronsCount);
                vm.MatrixPreview.Controller.SelectMatrix(_layerNum, MatrixTypes.Weights);
            }
            else if (e.PropertyName == nameof(model.InputsCount))
            {
                _networkService.SetInputsCount(model.InputsCount);
                vm.MatrixPreview.Controller.SelectMatrix(_layerNum, MatrixTypes.Weights);
            }
        }
    }
}