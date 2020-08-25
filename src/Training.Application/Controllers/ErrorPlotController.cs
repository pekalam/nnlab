using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Common.Framework;
using Common.Framework.Extensions;
using CommonServiceLocator;
using OxyPlot;
using Prism.Ioc;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;


namespace Training.Application.Services
{
    public interface IErrorPlotService
    {
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IErrorPlotService, ErrorPlotService>().Register<ITransientController<ErrorPlotService>, ErrorPlotController>();

        }
    }

    internal class ErrorPlotService : IErrorPlotService, IService
    {
        public Action<NavigationContext> Navigated { get; set; }

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
            private NavigationParameters _parameters;

            public ErrorPlotRecNavParams(NavigationParameters parameters)
            {
                _parameters = parameters;
            }

            public string ParentRegion =>  _parameters.GetOrDefault<string>(nameof(ParentRegion));
            public List<DataPoint> Points => _parameters.GetOrDefault<List<DataPoint>>(nameof(Points));
            public bool EpochEnd => _parameters.GetOrDefault<bool>(nameof(EpochEnd), true);
        }

        public static ErrorPlotRecNavParams FromParams(NavigationParameters navParams)
        {
            return new ErrorPlotRecNavParams(navParams);
        }


    }

    class ErrorPlotController : ITransientController<ErrorPlotService>
    {
        private string ErrorPlotSettingsRegion;
        private readonly object _ptsLock = new object();
        private CancellationTokenSource _cts;
        private PlotEpochEndConsumer? _epochEndConsumer;

        private IRegionManager _rm;
        private ModuleState _moduleState;
        private IViewModelAccessor _accessor;

        public ErrorPlotController(IViewModelAccessor accessor, ModuleState moduleState, IRegionManager rm)
        {
            _accessor = accessor;
            _moduleState = moduleState;
            _rm = rm;

            _accessor.OnCreated<ErrorPlotViewModel>(() =>
            {
                var vm = _accessor.Get<ErrorPlotViewModel>();

                vm.IsActiveChanged += (sender, args) =>
                {
                    if (!(sender as ErrorPlotViewModel).IsActive)
                    {
                        _epochEndConsumer?.ForceStop();
                    }
                };
            });
        }

        public void Initialize(ErrorPlotService service)
        {
            service.Navigated = Navigated;
        }

        private void InvalidatePlot()
        {
            var vm = _accessor.Get<ErrorPlotViewModel>();

            GlobalDistributingDispatcher.Call(() =>
            {
                if (System.Windows.Application.Current == null) return;


                System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    lock (_ptsLock)
                    {
                        vm.BasicPlotModel.Model.InvalidatePlot(true);
                    }
                }, DispatcherPriority.Background, _cts.Token);


            }, _epochEndConsumer);
        }

        private void InitPlotEpochEndConsumer()
        {
            var vm = _accessor.Get<ErrorPlotViewModel>();

            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState,(endsObs, session) =>
            {
                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                var points = endsObs.Select(end => new DataPoint(end.Epoch, end.Error));

                lock (_ptsLock)
                {
                    if (vm.Series.Points.Count + endsObs.Count > 2000)
                    {
                        var newMin = vm.Series.Points.Count + endsObs.Count;
                        vm.Series.Points.Clear();
                        vm.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
                    }
                    vm.Series.Points.AddRange(points);
                }

                InvalidatePlot();

            }, onTrainingStopped: _ =>
            {
                _cts.Cancel();
                // foreach (var op in ops)
                // {
                //     if(Application.Current != null)
                //         op.Abort();
                // }
                // ops.Clear();
            }, onTrainingStarting:
                 _ =>
                 {
                     _cts = new CancellationTokenSource();

                     InvalidatePlot();

                 }, onTrainingPaused: _ =>
                 {
                     _cts.Cancel();
                     // foreach (var op in ops)
                     // {
                     //     if (Application.Current != null)
                     //         op.Abort();
                     // }
                     // ops.Clear();
                 });

            var p = new NavigationParameters();
            p.Add(nameof(PlotEpochEndConsumer), _epochEndConsumer);


            _rm.Regions[ErrorPlotSettingsRegion].RemoveAll();
            _rm.Regions[ErrorPlotSettingsRegion].RequestNavigate("PlotEpochParametersView", p);

            _epochEndConsumer.Initialize();
        }

        private void Navigated(NavigationContext ctx)
        {
            var navParams = ErrorPlotNavParams.FromParams(ctx.Parameters);
            var vm = _accessor.Get<ErrorPlotViewModel>();

            ErrorPlotSettingsRegion = nameof(ErrorPlotSettingsRegion) + navParams.ParentRegion;
            vm.BasicPlotModel.SetSettingsRegion(ErrorPlotSettingsRegion);

            if (navParams.EpochEnd)
            {
                InitPlotEpochEndConsumer();
            }
            else
            {
                vm.Series.Points.Clear();
                vm.Series.Points.AddRange(navParams.Points);
                vm.BasicPlotModel.Model.InvalidatePlot(true);
            }
        }
    }
}
