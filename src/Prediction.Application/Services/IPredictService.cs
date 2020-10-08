using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using NNLibAdapter;
using Prediction.Application.Controllers;
using Prediction.Application.ViewModels;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;

namespace Prediction.Application.Services
{
    public interface IPredictService : IService
    {
        DelegateCommand PredictCommand { get; }
        Action<NavigationContext> Navigated { get; }


        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IPredictService, PredictService>().Register<ITransientController<PredictService>, PredictController>();

        }
    }

    public class PredictService : IPredictService
    {
        private IViewModelAccessor _accessor;

        public PredictService(IViewModelAccessor accessor, ITransientController<PredictService> ctrl)
        {
            _accessor = accessor;
            ctrl.Initialize(this);
        }

        public DelegateCommand PredictCommand { get; set; } = null!;
        public Action<NavigationContext> Navigated { get; set; } = null!;

        public void UpdateNetworkAndMatrix(MLPNetwork network, TrainingData data, Matrix<double> inputMatrix)
        {

            var vm = _accessor.Get<PredictViewModel>()!;

            var adapter = new NNLibModelAdapter();
            adapter.SetNeuralNetwork(network);
            adapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";
            vm.ModelAdapter = adapter;
            vm.ModelAdapter.SetInputLabels(data.Variables.InputVariableNames);
            vm.ModelAdapter.SetOutputLabels(data.Variables.TargetVariableNames);

            vm.InputMatrixVm.Controller.AssignMatrix(inputMatrix, new []{"Value"}, i => data.Variables.InputVariableNames[i]);
            if (network.Layers[^1].Output == null) return;
            vm.OutputMatrixVm.Controller.AssignMatrix(network.Layers[^1].Output!, new[] { "Value" }, i => data.Variables.TargetVariableNames[i]);
        }

        public void UpdateMatrix(MLPNetwork network, TrainingData data, Matrix<double> inputMatrix)
        {
            if (network.Layers[^1].Output == null) return;

            var vm = _accessor.Get<PredictViewModel>()!;
            vm.InputMatrixVm.Controller.AssignMatrix(inputMatrix, new[] { "Value" }, i => data.Variables.InputVariableNames[i]);
            if (network.Layers[^1].Output == null) return;
            vm.OutputMatrixVm.Controller.AssignMatrix(network.Layers[^1].Output!, new[] { "Value" }, i => data.Variables.TargetVariableNames[i]);
        }
    }
}
