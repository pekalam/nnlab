using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Common.Domain;
using Common.Framework;
using Common.Framework.Extensions;
using CommonServiceLocator;
using OxyPlot;
using Prism.Ioc;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.Plots;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Application.ViewModels.PanelLayout;
using Training.Domain;
// ReSharper disable InconsistentlySynchronizedField


namespace Training.Application.Services
{
    public interface IErrorPlotService : IService
    {
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IErrorPlotService, ErrorPlotService>()
                .Register<ITransientController<ErrorPlotService>, ErrorPlotController>();
        }
    }

    internal class ErrorPlotService : IErrorPlotService
    {
        public Action<NavigationContext> Navigated { get; set; } = null!;

        public ErrorPlotService(ITransientController<ErrorPlotService> ctrl)
        {
            ctrl.Initialize(this);
        }
    }
}

namespace Training.Application.Controllers
{
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

    class ErrorPlotController : ControllerBase<ErrorPlotViewModel>,ITransientController<ErrorPlotService>
    {
        private string _errorPlotSettingsRegion = null!;
        private readonly object _ptsLock = new object();
        private CancellationTokenSource? _cts;
        private PlotEpochEndConsumer? _epochEndConsumer;

        private readonly IRegionManager _rm;
        private readonly ModuleState _moduleState;


        public ErrorPlotController(IViewModelAccessor accessor, ModuleState moduleState, IRegionManager rm) : base(accessor)
        {
            _moduleState = moduleState;
            _rm = rm;
            _moduleState.ActiveSessionChanged += ModuleStateOnActiveSessionChanged;
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

        public void Initialize(ErrorPlotService service)
        {
            service.Navigated = Navigated;
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            //do not change for report view
            if (_epochEndConsumer == null) return;

            Vm!.Series.Points.Clear();

            var points = e.next.EpochEndEvents.TakeLast(2000).Select(end => new DataPoint(end.Epoch, end.Error))
                .ToArray();

            var newMin = e.next.EpochEndEvents.Count - 2000;
            Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
            Vm!.Series.Points.AddRange(points);

            Vm!.BasicPlotModel.Model.InvalidatePlot(true);
        }

        private void InvalidatePlot()
        {
            GlobalDistributingDispatcher.Call(() =>
            {
                lock (_ptsLock)
                {
                    Vm!.BasicPlotModel.Model.InvalidatePlot(true);
                }
            }, _epochEndConsumer!);
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


            _rm.Regions[_errorPlotSettingsRegion].RemoveAll();
            _rm.Regions[_errorPlotSettingsRegion].RequestNavigate("PlotEpochParametersView", p);

            _epochEndConsumer.Initialize();
        }

        private void Navigated(NavigationContext ctx)
        {
            var navParams = ErrorPlotNavParams.FromParams(ctx.Parameters);

            _errorPlotSettingsRegion = nameof(_errorPlotSettingsRegion) + navParams.ParentRegion;
            Vm!.BasicPlotModel.SetSettingsRegion(_errorPlotSettingsRegion);

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
    }
}