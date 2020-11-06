using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib.Common;
using NNLib.Data;
using NNLib.MLP;
using NNLibAdapter;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prediction.Application.Services;
using Prism.Regions;
using SharedUI.BasicPlot;
using SharedUI.MatrixPreview;

namespace Prediction.Application.ViewModels
{
    public class PredictViewModel : ViewModelBase<PredictViewModel>
    {
        private NNLibModelAdapter? _modelAdapter;
        private bool _showPlotPrediction;
        private double _startValue;
        private double _endValue;
        private double _interval = 0.25;
        private DataSetType[]? _plotSetTypes;
        private DataSetType _selectedPlotSetType;
        private MatrixPreviewViewModel _outputMatrixVm = new MatrixPreviewViewModel();

#pragma warning disable 8618
        public PredictViewModel()
#pragma warning restore 8618
        {
            
        }

        public PredictViewModel(IPredictService service)
        {
            Service = service;
            PlotModel = new BasicPlotModel();
            PlotModel.Model.Series.Add(DataPredictionLineSeries);
            PlotModel.Model.Series.Add(DataScatterSeries);
            PlotModel.Model.Series.Add(PredictionLineSeries);
            PlotModel.Model.Series.Add(PredictionScatterSeries);

            var l = new Legend
            {
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendPosition = LegendPosition.RightTop,
                LegendPlacement = LegendPlacement.Inside,
            };

            PlotModel.Model.Legends.Add(l);

            service.Initialize(this);
        }

        public NNLibModelAdapter? ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }

        public IPredictService Service { get; }

        public MatrixPreviewViewModel InputMatrixVm { get; set; } = new MatrixPreviewViewModel();

        public MatrixPreviewViewModel OutputMatrixVm
        {
            get => _outputMatrixVm;
            set => SetProperty(ref _outputMatrixVm, value);
        }

        public BasicPlotModel PlotModel { get; set; }

        public LineSeries DataPredictionLineSeries { get; set; } = new LineSeries()
        {
            Title = "Prediction"
        };

        public ScatterSeries DataScatterSeries { get; set; } = new ScatterSeries()
        {
            Title = "Original data",
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColor.FromRgb(0, 0, 255),
        };

        public LineSeries PredictionLineSeries { get; set; } = new LineSeries()
        {
            Title = "Prediction"
        };


        public ScatterSeries PredictionScatterSeries { get; set; } = new ScatterSeries()
        {
            Title = "Prediction",
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColor.FromRgb(0, 255, 0),
        };

        public DataSetType[]? PlotSetTypes
        {
            get => _plotSetTypes;
            set => SetProperty(ref _plotSetTypes,value);
        }

        public DataSetType SelectedPlotSetType
        {
            get => _selectedPlotSetType;
            set => SetProperty(ref _selectedPlotSetType, value);
        }

        public bool ShowPlotPrediction
        {
            get => _showPlotPrediction;
            set => SetProperty(ref _showPlotPrediction, value);
        }

        public double StartValue
        {
            get => _startValue;
            set => SetProperty(ref _startValue, value);
        }

        public double EndValue
        {
            get => _endValue;
            set => SetProperty(ref _endValue, value);
        }

        public double Interval
        {
            get => _interval;
            set
            {
                if(value <= 0) throw new Exception("Interval must be greater than 0");
                SetProperty(ref _interval, value);
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated?.Invoke(navigationContext);
        }


        public void UpdateNetworkAndMatrix(MLPNetwork network, TrainingData data, Matrix<double> inputMatrix)
        {


            var adapter = new NNLibModelAdapter();
            adapter.SetNeuralNetwork(network);
            adapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";
            ModelAdapter = adapter;
            ModelAdapter.SetInputLabels(data.Variables.InputVariableNames);
            ModelAdapter.SetOutputLabels(data.Variables.TargetVariableNames);

            InputMatrixVm.Controller.AssignMatrix(inputMatrix, new[] { "Value" }, i => data.Variables.InputVariableNames[i]);
            OutputMatrixVm = new MatrixPreviewViewModel();
            if (network.Layers[^1].Output == null) return;
            OutputMatrixVm.Controller.AssignMatrix(Matrix<double>.Build.Dense(network.Layers[^1].NeuronsCount, 1, 0), new[] { "Value" }, i => data.Variables.TargetVariableNames[i]);
        }

        public void UpdateMatrix(Matrix<double> output, TrainingData data, Matrix<double> inputMatrix)
        {
            InputMatrixVm.Controller.AssignMatrix(inputMatrix, new[] { "Value" }, i => data.Variables.InputVariableNames[i]);
            OutputMatrixVm.Controller.AssignMatrix(output, new[] { "Value" }, i => data.Variables.TargetVariableNames[i]);
        }


        public void UpdatePlots(ScatterPoint[] dataScatter, DataPoint[] dataPredLine, DataPoint[] predLine, ScatterPoint[] predScatter)
        {

            ClearPlots();

            DataScatterSeries.Points.AddRange(dataScatter);
            DataPredictionLineSeries.Points.AddRange(dataPredLine);
            PredictionLineSeries.Points.AddRange(predLine);
            PredictionScatterSeries.Points.AddRange(predScatter);

            PlotModel.Model.DefaultXAxis.Reset();
            PlotModel.Model.DefaultYAxis.Reset();

            PlotModel.Model.InvalidatePlot(true);
        }

        public void ClearPlots()
        {

            DataScatterSeries.Points.Clear();
            DataPredictionLineSeries.Points.Clear();
            PredictionLineSeries.Points.Clear();
            PredictionScatterSeries.Points.Clear();
            PlotModel.Model.InvalidatePlot(true);
        }

        public void UpdateAxes(TrainingData trainingData)
        {
            PlotModel.Model.Axes.Clear();
            var inputVarInd = trainingData.Variables.Indexes.InputVarIndexes[0];
            var targetVarInd = trainingData.Variables.Indexes.TargetVarIndexes[0];

            PlotModel.Model.Axes.Add(new LinearAxis()
            {
                Title = trainingData.Variables.Names[inputVarInd],
                Position = AxisPosition.Bottom,
            });
            PlotModel.Model.Axes.Add(new LinearAxis()
            {
                Title = trainingData.Variables.Names[targetVarInd],
                Position = AxisPosition.Left
            });
        }
    }
}
