using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using NNLib.Common;
using NNLibAdapter;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prediction.Application.Controllers;
using Prediction.Application.ViewModels;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using SharedUI.MatrixPreview;

namespace Prediction.Application.Services
{
    public interface IPredictService : IService
    {
        DelegateCommand PredictCommand { get; }
        Action<NavigationContext> Navigated { get; set; }

        DelegateCommand<DataSetType?> PredictPlotCommand { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IPredictService, PredictService>().Register<ITransientController<PredictService>, PredictController>();

        }
    }

    public class PredictService : IPredictService
    {
        private readonly IViewModelAccessor _accessor;

        public PredictService(IViewModelAccessor accessor, ITransientController<PredictService> ctrl)
        {
            _accessor = accessor;
            ctrl.Initialize(this);
        }

        public DelegateCommand PredictCommand { get; set; } = null!;
        public Action<NavigationContext> Navigated { get; set; } = null!;
        public DelegateCommand<DataSetType?> PredictPlotCommand { get; set; } = null!;

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
            vm.OutputMatrixVm = new MatrixPreviewViewModel();
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


        public void UpdatePlots(ScatterPoint[] dataScatter, DataPoint[] dataPredLine, DataPoint[] predLine, ScatterPoint[] predScatter)
        {
            var vm = _accessor.Get<PredictViewModel>()!;

            vm.DataScatterSeries.Points.Clear();
            vm.DataPredictionLineSeries.Points.Clear();
            vm.PredictionLineSeries.Points.Clear();
            vm.PredictionScatterSeries.Points.Clear();

            vm.DataScatterSeries.Points.AddRange(dataScatter);
            vm.DataPredictionLineSeries.Points.AddRange(dataPredLine);
            vm.PredictionLineSeries.Points.AddRange(predLine);
            vm.PredictionScatterSeries.Points.AddRange(predScatter);

            vm.PlotModel.Model.DefaultXAxis.Reset();
            vm.PlotModel.Model.DefaultYAxis.Reset();

            vm.PlotModel.Model.InvalidatePlot(true);
        }

        public void ClearPlots()
        {
            var vm = _accessor.Get<PredictViewModel>()!;

            vm.DataScatterSeries.Points.Clear();
            vm.DataPredictionLineSeries.Points.Clear();
            vm.PredictionLineSeries.Points.Clear();
            vm.PredictionScatterSeries.Points.Clear();
        }

        public void UpdateAxes(TrainingData trainingData)
        {
            var vm = _accessor.Get<PredictViewModel>();
            vm!.PlotModel.Model.Axes.Clear();
            var inputVarInd = trainingData.Variables.Indexes.InputVarIndexes[0];
            var targetVarInd = trainingData.Variables.Indexes.TargetVarIndexes[0];

            vm!.PlotModel.Model.Axes.Add(new LinearAxis()
            {
                Title = trainingData.Variables.Names[inputVarInd],
                Position = AxisPosition.Bottom,
            });
            vm!.PlotModel.Model.Axes.Add(new LinearAxis()
            {
                Title = trainingData.Variables.Names[targetVarInd],
                Position = AxisPosition.Left
            });
        }
    }
}
