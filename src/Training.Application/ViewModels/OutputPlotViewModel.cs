using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib.Data;
using NNLib.MLP;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Regions;
using SharedUI.BasicPlot;
using System.Collections.Generic;
using System.Threading;
using Training.Application.Controllers;
using Training.Domain;

namespace Training.Application.ViewModels
{
    public enum OutputPlotType
    {
        Approximation, VecNum
    }

    public class OutputPlotViewModel : ViewModelBase<OutputPlotViewModel>
    {
        private OutputPlotType _plotType;

        public OutputPlotViewModel(IOutputPlotController service)
        {
            Service = service;
            service.Initialize(this);
        }

        public IOutputPlotController Service { get; }

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
        private TwoColorAreaSeries _output = null!;

        public void OnEpochEnd(IList<EpochEndArgs> args, OutputPlotViewModel vm, CancellationToken ct)
        {
            var network = _session.Network!.Clone();
            var input = _session.TrainingData!.Sets.TrainingSet.Input;
            var target = _session.TrainingData!.Sets.TrainingSet.Target;


            var dataPoints = new DataPoint[input.Count];
            for (int i = 0; i < input.Count; i++)
            {
                network.CalculateOutput(input[i]);

                dataPoints[i] = new DataPoint(i, target[i][0,0] - network.Output![0, 0]);
            }
            _output.Points.Clear();
            _output.Points.AddRange(dataPoints);
        }

        public void OnSessionStarting(OutputPlotViewModel vm, TrainingSession session, CancellationToken ct)
        {
            _session = session;

            vm.PlotType = OutputPlotType.VecNum;
            vm.PlotModel.Series.Clear();
            vm.PlotModel.Title = "Network error: Target - output";

            var input = _session.TrainingData!.Sets.TrainingSet.Input;

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
                Title = "Network error",
                Position = AxisPosition.Left,
            });

            var scatter = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColor.FromRgb(0, 0, 255),
            };

            vm.PlotModel.Series.Add(scatter);

            _output = new TwoColorAreaSeries()
            {
                Color = OxyColor.FromRgb(255, 0, 0),
                Color2 = OxyColor.FromRgb(255, 0, 0),
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
        }

        public void SetOutput(SupervisedTrainingSamples set, Matrix<double>[] output)
        {
            throw new System.NotImplementedException();
        }
    }
}
