﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Common.Domain;
using NNLib.Data;
using NNLib.MLP;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Training.Domain;

namespace Training.Application.ViewModels
{
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