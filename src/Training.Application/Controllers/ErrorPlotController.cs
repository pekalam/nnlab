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
// ReSharper disable InconsistentlySynchronizedField

namespace Training.Application.Controllers
{
    public interface IErrorPlotController : ITransientController
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

            var points = e.next.EpochEndEvents.Skip(e.next.EpochEndEvents.Count / 2000 * 2000).Select(end => new DataPoint(end.Epoch, end.Error))
                .ToArray();


            double newMin = e.next.EpochEndEvents.Count / 2000 * 2000;
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
            GlobalDistributingDispatcher.CallDirectly(InvalidateMethod, _epochEndConsumer!);
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
                        if (Vm!.Series.Points.Count + endsObs.Count > 2000)
                        {
                            var newMin = endsObs[^1].Epoch;
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
            p.Add("PlotModel", Vm!.BasicPlotModel);

            _rm.Regions[_errorPlotSettingsRegion].RemoveAll();
            _rm.Regions[_errorPlotSettingsRegion].RequestNavigate("ErrorPlotSettingsView", p);

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