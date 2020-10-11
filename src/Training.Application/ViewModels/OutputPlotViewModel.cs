using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using NNLib.Common;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Regions;
using SharedUI.BasicPlot;
using Training.Application.Services;
using Training.Domain;
using System.Linq;

namespace Training.Application.ViewModels
{
    public enum OutputPlotType
    {
        Approximation, VecNum
    }

    public class OutputPlotViewModel : ViewModelBase<OutputPlotViewModel>
    {
        private OutputPlotType _plotType;

        public OutputPlotViewModel(IOutputPlotService service)
        {
            Service = service;
        }

        public IOutputPlotService Service { get; }

        public OutputPlotType PlotType
        {
            get => _plotType;
            set => SetProperty(ref _plotType, value);
        }

        public BasicPlotModel BasicPlotModel { get; } = new BasicPlotModel();

        public PlotModel PlotModel => BasicPlotModel.Model;
        public PlotController PlotController => BasicPlotModel.Controller;

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }




    interface IOutputPlot
    {
        void OnEpochEnd(IList<EpochEndArgs> args, OutputPlotViewModel vm, CancellationToken ct);
        void OnSessionStarting(OutputPlotViewModel vm, TrainingSession session, CancellationToken ct);
        void OnSessionStopped(TrainingSession session);
        void OnSessionPaused(TrainingSession session);
        void GeneratePlot(DataSetType set, TrainingData trainingData, MLPNetwork net, OutputPlotViewModel vm);
    }

    internal class VecNumPlot : IOutputPlot
    {
        private TrainingSession _session = null!;
        private ScatterSeries _output = null!;

        public void OnEpochEnd(IList<EpochEndArgs> args, OutputPlotViewModel vm, CancellationToken ct)
        {
            _output.Points.Clear();
            var input = _session.TrainingData!.Sets.TrainingSet.Input;


            var dataPoints = new List<ScatterPoint>();
            for (int i = 0; i < input.Count; i++)
            {
                _session.Network!.CalculateOutput(input[i]);

                var netOutput = _session.Network.Output!;

                dataPoints.Add(new ScatterPoint(i, netOutput[0, 0]));
            }
            _output.Points.AddRange(dataPoints);
        }

        public void OnSessionStarting(OutputPlotViewModel vm, TrainingSession session, CancellationToken ct)
        {
            _session = session;

            vm.PlotType = OutputPlotType.VecNum;
            vm.PlotModel.Series.Clear();
            vm.PlotModel.Title = "Accuracy";

            var input = _session.TrainingData!.Sets.TrainingSet.Input;
            var target = session.TrainingData!.Sets.TrainingSet.Target;
            var targetVarInd = session.TrainingData.Variables.Indexes.TargetVarIndexes;

            vm.PlotModel.Axes.Clear();
            vm.PlotModel.Axes.Add(new LinearAxis()
            {
                Title = "Nth input vector",
                Position = AxisPosition.Bottom,
                MinimumMajorStep = 1,
                MinimumMinorStep = 1,
                AbsoluteMinimum = 0,
                AbsoluteMaximum = input.Count,
            });
            vm.PlotModel.Axes.Add(new LinearAxis()
            {
                Title = session.TrainingData.Variables.Names[targetVarInd[0]],
                Position = AxisPosition.Left,
            });

            var scatter = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColor.FromRgb(0, 0, 255),
            };

            for (int i = 0; i < input.Count; i++)
            {
                scatter.Points.Add(new ScatterPoint(i, target[i][0, 0]));
            }

            vm.PlotModel.Series.Add(scatter);

            _output = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColor.FromRgb(255, 0, 0),
            };
            vm.PlotModel.Series.Add(_output);
        }

        public void OnSessionStopped(TrainingSession session)
        {

        }

        public void OnSessionPaused(TrainingSession session)
        {

        }

        public void GeneratePlot(DataSetType set, TrainingData trainingData, MLPNetwork net, OutputPlotViewModel vm)
        {
            throw new System.NotImplementedException();
        }

        public void SetOutput(SupervisedSet set, Matrix<double>[] output)
        {
            throw new System.NotImplementedException();
        }
    }

    internal class OutputPlotSelector
    {
        public void SelectPlot(TrainingSession session) => SelectPlot(session.Network!);

        public void SelectPlot(MLPNetwork network)
        {
            if (network.Layers[0].InputsCount == 1 && network.Layers[^1].NeuronsCount == 1)
            {
                OutputPlot = new ApproximationOutputPlot();
            }
            else if (network.Layers[^1].NeuronsCount == 1)
            {
                OutputPlot = new VecNumPlot();
            }
        }

        public IOutputPlot? OutputPlot { get; private set; }
    }

    internal class ApproximationOutputPlot : IOutputPlot
    {
        private LineSeries? _output;
        private TrainingSession? _session;

        public void OnEpochEnd(IList<EpochEndArgs> args, OutputPlotViewModel vm, CancellationToken ct)
        {
            var network = _session!.Network!.Clone();

            var dataPoints = new List<DataPoint>();
            var input = _session.TrainingData!.Sets.TrainingSet.Input;

            for (int i = 0; i < input.Count; i++)
            {
                network.CalculateOutput(input[i]);
                dataPoints.Add(new DataPoint(input[i].At(0, 0), network.Output!.At(0, 0)));
            }
            _output!.Points.Clear();
            _output.Points.AddRange(dataPoints.OrderBy(p => p.X));
        }

        public void OnSessionStarting(OutputPlotViewModel vm, TrainingSession session, CancellationToken ct)
        {
            _session = session;
            InitPlot(session.TrainingData!, DataSetType.Training, vm);
        }

        private void InitPlot(TrainingData trainingData, DataSetType setType, OutputPlotViewModel vm)
        {

            vm.PlotType = OutputPlotType.Approximation;
            vm.PlotModel.Series.Clear();
            vm.PlotModel.Title = "Accuracy";


            var inputVarInd = trainingData.Variables.Indexes.InputVarIndexes[0];
            var targetVarInd = trainingData.Variables.Indexes.TargetVarIndexes[0];

            vm.PlotModel.Axes.Clear();
            vm.PlotModel.Axes.Add(new LinearAxis()
            {
                Title = trainingData.Variables.Names[inputVarInd],
                Position = AxisPosition.Bottom,
            });
            vm.PlotModel.Axes.Add(new LinearAxis()
            {
                Title = trainingData.Variables.Names[targetVarInd],
                Position = AxisPosition.Left
            });

            var scatter = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColor.FromRgb(0, 0, 255),
            };




            _output = new LineSeries()
            {
                Color = OxyColor.FromRgb(255, 0, 0),
            };

            var input = trainingData.GetSet(setType)!.Input;
            var target = trainingData.GetSet(setType)!.Target;

            for (int i = 0; i < input.Count; i++)
            {
                scatter.Points.Add(new ScatterPoint(input[i][0, 0], target[i][0, 0]));
            }

            vm.PlotModel.Series.Add(scatter);
            vm.PlotModel.Series.Add(_output);
        }

        public void OnSessionStopped(TrainingSession session)
        {
        }

        public void OnSessionPaused(TrainingSession session) => OnSessionStopped(session);

        public void GeneratePlot(DataSetType set, TrainingData trainingData, MLPNetwork net, OutputPlotViewModel vm)
        {
            if (_output == null)
            {
                _output = new LineSeries()
                {
                    Color = OxyColor.FromRgb(255, 0, 0),
                };
                vm.PlotModel.Series.Add(_output);
            }
            else
            {
                _output.Points.Clear();
            }


            InitPlot(trainingData, set, vm);

            var dataPoints = new List<DataPoint>();
            var input = trainingData.GetSet(set)!.Input;

            for (int i = 0; i < input.Count; i++)
            {
                net.CalculateOutput(input[i]);
                dataPoints.Add(new DataPoint(input[i].At(0, 0), net.Output!.At(0, 0)));
            }
            _output.Points.Clear();
            _output.Points.AddRange(dataPoints);
        }
    }
}
