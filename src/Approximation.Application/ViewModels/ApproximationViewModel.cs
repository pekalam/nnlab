using System;
using Approximation.Application.Controllers;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib.Data;
using NNLib.MLP;
using NNLibAdapter;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using Prism.Regions;
using SharedUI.BasicPlot;
using SharedUI.MatrixPreview;

namespace Approximation.Application.ViewModels
{
    public class ApproximationViewModel : ViewModelBase<ApproximationViewModel>
    {
        private NNLibModelAdapter? _modelAdapter;
        private bool _showPlotPrediction;
        private double _startValue;
        private double _endValue;
        private double _interval = 0.25;
        private DataSetType[]? _plotSetTypes;
        private DataSetType _selectedPlotSetType;
        private MatrixPreviewViewModel _outputMatrixVm = new MatrixPreviewViewModel();
        private bool _showCustomApproximation = true;
        private bool _showApproximation = true;
        private string? _warningMessage;

#pragma warning disable 8618
        public ApproximationViewModel()
#pragma warning restore 8618
        {
            
        }

        public ApproximationViewModel(IApproximationController controller)
        {
            Controller = controller;
            PlotModel = new BasicPlotModel();
            PlotModel.DisplaySettingsRegion = false;
            PlotModel.Model.Series.Add(DataPredictionLineSeries);
            PlotModel.Model.Series.Add(DataScatterSeries);
            PlotModel.Model.Series.Add(PredictionLineSeries);
            PlotModel.Model.Series.Add(PredictionScatterSeries);
            PlotModel.Model.Series.Add(DataPredictionScatter);

            var l = new Legend
            {
                LegendBorder = OxyColors.Black,
                LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                LegendPosition = LegendPosition.RightTop,
                LegendPlacement = LegendPlacement.Inside,
            };

            PlotModel.Model.Legends.Add(l);

            controller.Initialize(this);
        }

        public NNLibModelAdapter? ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }

        public IApproximationController Controller { get; }

        public MatrixPreviewViewModel InputMatrixVm { get; set; } = new MatrixPreviewViewModel();

        public MatrixPreviewViewModel OutputMatrixVm
        {
            get => _outputMatrixVm;
            set => SetProperty(ref _outputMatrixVm, value);
        }

        public BasicPlotModel PlotModel { get; set; }

        public LineSeries DataPredictionLineSeries { get; set; } = new LineSeries()
        {
            Title = "Approximation of network output",
            Color = OxyColor.FromRgb(255,0,0),
        };

        public ScatterSeries DataScatterSeries { get; set; } = new ScatterSeries()
        {
            Title = "Training data",
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColor.FromRgb(0, 0, 255),
        };

        public LineSeries PredictionLineSeries { get; set; } = new LineSeries()
        {
            Title = "Approximation of network output (custom)",
            Color = OxyColor.FromRgb(0,131,0),
        };


        public ScatterSeries PredictionScatterSeries { get; set; } = new ScatterSeries()
        {
            Title = "Network output (custom)",
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColor.FromRgb(0, 211, 0),
        };

        public ScatterSeries DataPredictionScatter { get; set; } = new ScatterSeries
        {
            Title = "Network output",
            MarkerType = MarkerType.Circle,
            MarkerSize = 3,
            MarkerFill = OxyColor.FromRgb(131, 0, 0),
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

        public string? WarningMessage
        {
            get => _warningMessage;
            set => SetProperty(ref _warningMessage, value);
        }

        public bool ShowCustomApproximation
        {
            get => _showCustomApproximation;
            set
            {
                SetProperty(ref _showCustomApproximation, value);
                PredictionScatterSeries.IsVisible = value;
                PredictionLineSeries.IsVisible = value;
                PlotModel.Model.InvalidatePlot(false);
            }
        }

        public bool ShowApproximation
        {
            get => _showApproximation;
            set
            {
                SetProperty(ref _showApproximation, value);
                DataPredictionLineSeries.IsVisible = value;
                DataPredictionScatter.IsVisible = value;
                PlotModel.Model.InvalidatePlot(false);
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Controller.Navigated?.Invoke(navigationContext);
        }


        public void UpdateNetworkAndMatrix(MLPNetwork network, TrainingData data, Matrix<double> inputMatrix)
        {
            if (data.Variables.InputVariableNames.Length != network.BaseLayers[0].InputsCount ||
                data.Variables.TargetVariableNames.Length != network.BaseLayers[^1].NeuronsCount) return;

            var adapter = new NNLibModelAdapter(network);
            adapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";
            ModelAdapter = adapter;
            ModelAdapter.AttachInputLabels(data.Variables.InputVariableNames);
            ModelAdapter.AttachOutputLabels(data.Variables.TargetVariableNames);

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


        public void UpdatePlots(TrainingData data,ScatterPoint[] dataScatter, ScatterPoint[] dataPredScatterPoints, DataPoint[] dataPredLine, DataPoint[] predLine, ScatterPoint[] predScatter)
        {
            ClearPlots(false);

            if (PlotModel.Model.Axes.Count == 0)
            {
                UpdateAxes(data);
            }

            DataScatterSeries.Points.AddRange(dataScatter);
            DataPredictionLineSeries.Points.AddRange(dataPredLine);
            PredictionLineSeries.Points.AddRange(predLine);
            PredictionScatterSeries.Points.AddRange(predScatter);
            DataPredictionScatter.Points.AddRange(dataPredScatterPoints);



            PlotModel.Model.Axes[0].Reset();
            PlotModel.Model.Axes[1].Reset();

            PlotModel.Model.InvalidatePlot(true);
        }

        public void ClearPlots(bool invalidate = true)
        {
            DataScatterSeries.Points.Clear();
            DataPredictionLineSeries.Points.Clear();
            PredictionLineSeries.Points.Clear();
            PredictionScatterSeries.Points.Clear();
            DataPredictionScatter.Points.Clear();
            if (invalidate)
            {
                PlotModel.Model.InvalidatePlot(true);
            }
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
