using Common.Framework;
using Common.Framework.Extensions;
using OxyPlot;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Training.Application.Plots;
using Training.Application.ViewModels;
using Training.Domain;
using IController = Common.Framework.IController;

// ReSharper disable InconsistentlySynchronizedField

namespace Training.Application.Controllers
{
    public interface IErrorPlotController : IController
    {
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IErrorPlotController, ErrorPlotController>();
        }
    }


    class ErrorPlotNavParams : NavigationParameters
    {
        public ErrorPlotNavParams(string parentRegion, bool epochEnd, List<DataPoint> points)
        {
            Add(nameof(ErrorPlotRecNavParams.ParentRegion), parentRegion);
            Add(nameof(ErrorPlotRecNavParams.Points), points);
            Add(nameof(ErrorPlotRecNavParams.EpochEnd), epochEnd);
        }

        public class ErrorPlotRecNavParams
        {
            private readonly NavigationParameters _parameters;

            public ErrorPlotRecNavParams(NavigationParameters parameters)
            {
                _parameters = parameters;
            }

            public string ParentRegion => _parameters.GetOrDefault<string>(nameof(ParentRegion));
            public List<DataPoint> Points => _parameters.GetOrDefault<List<DataPoint>>(nameof(Points));
            public bool EpochEnd => _parameters.GetOrDefault<bool>(nameof(EpochEnd), true);
        }

        public static ErrorPlotRecNavParams FromParams(NavigationParameters navParams)
        {
            return new ErrorPlotRecNavParams(navParams);
        }
    }

    class ErrorPlotController : ControllerBase<ErrorPlotViewModel>,IErrorPlotController
    {
        private const int EpochEndMaxPoints = 10_000;
        private string _errorPlotSettingsRegion = null!;
        private readonly object _ptsLock = new object();
        private CancellationTokenSource? _cts;
        private PlotEpochEndConsumer? _epochEndConsumer;

        private readonly IRegionManager _rm;
        private readonly ModuleState _moduleState;


        public ErrorPlotController(ModuleState moduleState, IRegionManager rm)
        {
            _moduleState = moduleState;
            _rm = rm;
            _moduleState.ActiveSessionChanged += ModuleStateOnActiveSessionChanged;

            Navigated = NavigatedAction;
        }

        protected override void VmCreated()
        {
            Vm!.IsActiveChanged += OnIsActiveChanged;
        }

        private void OnIsActiveChanged(object? sender, EventArgs e)
        {
            if(!Vm!.IsActive)
            {
                _epochEndConsumer?.ForceStop();
                Vm!.IsActiveChanged -= OnIsActiveChanged;
            }
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            //do not change for report view
            if (_epochEndConsumer == null) return;

            e.next.SessionReset -= NextOnSessionReset;
            e.next.SessionReset += NextOnSessionReset;

            Vm!.Series.Points.Clear();

            var points = e.next.EpochEndEvents.Skip(e.next.EpochEndEvents.Count / EpochEndMaxPoints * EpochEndMaxPoints).Select(end => new DataPoint(end.Epoch, end.Error))
                .ToArray();


            double newMin = points.Length > 0 ? points[0].X : 0;
            Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
            Vm!.Series.Points.AddRange(points);

            Vm!.BasicPlotModel.Model.InvalidatePlot(true);
        }

        private void NextOnSessionReset()
        {
            Vm!.Series.Points.Clear();
        }

        private void InvalidatePlot()
        {
            GlobalDistributingDispatcher.CallCustom(InvalidateMethod, _epochEndConsumer!);
        }

        private void InvalidateMethod()
        {
            lock (_ptsLock)
            {
                Vm!.BasicPlotModel.Model.InvalidatePlot(true);
            }
        }

        private void InitPlotEpochEndConsumer()
        {

            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState, (endsObs, session) =>
                {
                    if (_cts!.IsCancellationRequested)
                    {
                        return;
                    }

                    var points = endsObs.Select(end => new DataPoint(end.Epoch, end.Error)).ToArray();
                    
                    lock (_ptsLock)
                    {
                        if (Vm!.Series.Points.Count == 0)
                        {
                            var newMin = endsObs[0].Epoch;
                            Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
                        }

                        if (Vm!.Series.Points.Count + endsObs.Count > EpochEndMaxPoints)
                        {
                            var newMin = endsObs[0].Epoch;
                            Vm!.Series.Points.Clear();
                            Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
                        }

                        Vm!.Series.Points.AddRange(points);
                    }

                    InvalidatePlot();
                }, onTrainingStopped: _ => { _cts!.Cancel(); }, 
                onTrainingStarting:
                _ =>
                {
                    _cts = new CancellationTokenSource();

                    InvalidatePlot();
                }, onTrainingPaused: _ => { _cts!.Cancel(); }, options: new PlotEpochEndConsumerOptions()
                {
                    DefaultConsumerType = PlotEpochEndConsumerType.Online,
                });

            var p = new NavigationParameters();
            p.Add(nameof(PlotEpochEndConsumer), _epochEndConsumer);

            _rm.Regions[_errorPlotSettingsRegion].RemoveAll();
            _rm.Regions[_errorPlotSettingsRegion].RequestNavigate("PlotEpochParametersView", p);

            _epochEndConsumer.Initialize();
        }

        private void NavigatedAction(NavigationContext ctx)
        {
            var navParams = ErrorPlotNavParams.FromParams(ctx.Parameters);

            _errorPlotSettingsRegion = nameof(_errorPlotSettingsRegion) + navParams.ParentRegion;
            Vm!.BasicPlotModel.SetSettingsRegion?.Invoke(_errorPlotSettingsRegion);

            if (navParams.EpochEnd)
            {
                InitPlotEpochEndConsumer();
            }
            else
            {
                Vm!.Series.Points.Clear();
                Vm!.Series.Points.AddRange(navParams.Points);
                Vm!.BasicPlotModel.Model.InvalidatePlot(true);
            }
        }

        public Action<NavigationContext> Navigated { get; }
    }
}