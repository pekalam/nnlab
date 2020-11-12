using System;
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
using OxyPlot.Annotations;
using OxyPlot.Legends;
using Training.Application.Controllers;
using Training.Domain;

namespace Training.Application.ViewModels
{
    public enum OutputPlotType
    {
        Approximation, VecNum,
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

        void CreateForSession(TrainingSession session, OutputPlotViewModel vm);
    }

    internal class VecNumPlot : IOutputPlot
    {
        private TrainingSession _session = null!;
        private ScatterSeries _output = null!;
        private ElementCollection<Annotation> _annotations;

        public void OnEpochEnd(IList<EpochEndArgs> args, OutputPlotViewModel vm, CancellationToken ct)
        {
            _output.Points.Clear();
            _annotations.Clear();

            var zeroLine = new LineAnnotation();
            zeroLine.Color = OxyColors.Black;
            zeroLine.MinimumY = 0;
            zeroLine.MinimumX = 0;
            zeroLine.MaximumY = 0;
            zeroLine.MaximumX = vm.PlotModel.Axes[0].AbsoluteMaximum;
            zeroLine.Y = 0;
            zeroLine.LineStyle = LineStyle.Solid;
            zeroLine.Type = LineAnnotationType.Horizontal;
            vm.PlotModel.Annotations.Add(zeroLine);


            var network = _session.Network!;
            var input = _session.TrainingData!.Sets.TrainingSet.Input;
            var target = _session.TrainingData!.Sets.TrainingSet.Target;


            var dataPoints = new ScatterPoint[input.Count];
            for (int i = 0; i < input.Count; i++)
            {
                network.CalculateOutput(input[i]);

                dataPoints[i] = new ScatterPoint(i, target[i][0,0] - network.Output![0, 0]);

                var annotation = new LineAnnotation();
                annotation.Color = OxyColors.Blue;
                annotation.MinimumY = 0;
                annotation.MaximumY = dataPoints[i].Y;
                annotation.X = dataPoints[i].X;
                annotation.LineStyle = LineStyle.Solid;
                annotation.Type = LineAnnotationType.Vertical;
                _annotations.Add(annotation);


            }
            _output.Points.AddRange(dataPoints);




        }

        public void OnSessionStarting(OutputPlotViewModel vm, TrainingSession session, CancellationToken ct)
        {

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

        public void CreateForSession(TrainingSession session, OutputPlotViewModel vm)
        {
            _session = session;

            vm.PlotType = OutputPlotType.VecNum;
            vm.PlotModel.Series.Clear();
            vm.PlotModel.Annotations.Clear();
            vm.PlotModel.Title = "Accuracy";

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
                Title = "Target - output",
                Position = AxisPosition.Left,
                AxisTitleDistance = 18,
            });


            var scatter = new ScatterSeries()
            {
                MarkerType = MarkerType.Circle,
                MarkerSize = 3,
                MarkerFill = OxyColor.FromRgb(0, 0, 255),
            };

            vm.PlotModel.Series.Add(scatter);
            _output = scatter;
            _annotations = vm.PlotModel.Annotations;
        }
    }
}
