using System;
using Common.Domain;
using NNLib.Data;
using NNLib.MLP;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Training.Domain;

namespace Training.Application.ViewModels
{
    internal class DataPointComparer : IComparer<DataPoint>
    {
        public int Compare(DataPoint x, DataPoint y)
        {
            return x.X.CompareTo(y.X);
        }
    }

    internal class ApproximationOutputPlot : IOutputPlot
    {
        private LineSeries? _output;
        private TrainingSession? _session;
        private readonly IComparer<DataPoint> _dataPointComparer = new DataPointComparer();

        public void OnEpochEnd(IList<EpochEndArgs> args, OutputPlotViewModel vm, CancellationToken ct)
        {
            PlotDataPoints(_session!.Network!);
        }

        private void PlotDataPoints(MLPNetwork network)
        {
            Debug.Assert(_session != null, nameof(_session) + " != null");

            var input = _session.TrainingData!.Sets.TrainingSet.Input;
            var orgInput = _session.TrainingData!.OriginalSets.TrainingSet.Input;
            var dataPoints = new DataPoint[input.Count];

            for (int i = 0; i < input.Count; i++)
            {
                network.CalculateOutput(input[i]);
                dataPoints[i] = new DataPoint(orgInput[i].At(0, 0), network.Output!.At(0, 0));
            }

            Array.Sort(dataPoints, _dataPointComparer);

            _output!.Points.Clear();
            _output.Points.AddRange(dataPoints);
        }

        public void OnSessionStarting(OutputPlotViewModel vm, TrainingSession session, CancellationToken ct)
        {
        }

        private void InitPlot(TrainingData trainingData, DataSetType setType, OutputPlotViewModel vm)
        {

            vm.PlotType = OutputPlotType.Approximation;
            vm.PlotModel.Series.Clear();
            vm.PlotModel.Annotations.Clear();
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
                Position = AxisPosition.Left,
                AxisTitleDistance = 18,
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

            var input = trainingData.GetOriginalSet(setType)!.Input;
            var target = trainingData.GetOriginalSet(setType)!.Target;

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

            PlotDataPoints(net);
        }

        public void CreateForSession(TrainingSession session, OutputPlotViewModel vm)
        {
            _session = session;
            InitPlot(session.TrainingData!, DataSetType.Training, vm);
        }
    }
}