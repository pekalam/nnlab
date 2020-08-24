using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Common.Domain;
using Common.Framework;
using CommonServiceLocator;
using NNLib;
using NNLib.Common;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Services
{
    public interface IOutputPlotService : IService
    {
        Action<NavigationContext> Navigated { get; set; }
    }

    internal class OutputPlotService : IOutputPlotService
    {
        public OutputPlotService(OutputPlotController ctrl)
        {
            ctrl.Initialize(this);
        }

        public Action<NavigationContext> Navigated { get; set; }
    }
}

namespace Training.Application.Controllers
{
    class OutputPlotSelector
    {
        public void SelectPlot(TrainingSession session) => SelectPlot(session.Network);

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

    class OutputPlotController : ITransientController<OutputPlotService>
    {
        public static string EpochSubNavParam = nameof(EpochSubNavParam);
        public static string NetNavParam = nameof(NetNavParam);
        public static string SetNavParam = nameof(SetNavParam);
        public static string DataNavParam = nameof(DataNavParam);
        public static string CtsNavParam = nameof(CtsNavParam);
        public static string ParentRegionNavParam = nameof(ParentRegionNavParam);

        private string OutputPlotSettingsRegion;

        private PlotEpochEndConsumer? _epochEndConsumer;
        private readonly OutputPlotSelector _plotSelector = new OutputPlotSelector();
        private CancellationTokenSource _cts;
        private readonly List<DispatcherOperation> _ops = new List<DispatcherOperation>();
        private IRegionManager _rm;
        private IEventAggregator _ea;
        private IViewModelAccessor _accessor;
        private readonly ModuleState _moduleState;

        public OutputPlotController(IRegionManager rm, IEventAggregator ea, IViewModelAccessor accessor, ModuleState moduleState)
        {
            _rm = rm;
            _ea = ea;
            _accessor = accessor;
            _moduleState = moduleState;

            _accessor.OnCreated<OutputPlotViewModel>(() =>
            {
                var vm = _accessor.Get<OutputPlotViewModel>();

                vm.IsActiveChanged += (sender, args) =>
                {
                    if (!(sender as OutputPlotViewModel).IsActive)
                    {
                        _epochEndConsumer?.ForceStop();
                    }
                };
            });
        }

        public void Initialize(OutputPlotService service)
        {
            service.Navigated = OnNavigated;
        }

        private void InvalidatePlot()
        {
            GlobalDistributingDispatcher.Call(() =>
            {
                var vm = _accessor.Get<OutputPlotViewModel>();

                if (System.Windows.Application.Current == null) return;

                var op = System.Windows.Application.Current.Dispatcher.InvokeAsync(() => vm.PlotModel.InvalidatePlot(true),
                    DispatcherPriority.Background, _cts.Token);
                //_ops.Add(op);

            }, _epochEndConsumer);
        }

        private void InitPlotEpochEndConsumer()
        {
            var vm = _accessor.Get<OutputPlotViewModel>();

            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState,(epochEnds, session) =>
            {
                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                _plotSelector.OutputPlot?.OnEpochEnd(epochEnds, vm, _cts.Token);

                if (System.Windows.Application.Current == null) return;

                InvalidatePlot();
            }, session =>
            {
                _cts = new CancellationTokenSource();
                _plotSelector.SelectPlot(session);
                _plotSelector.OutputPlot?.OnSessionStarting(vm, session, _cts.Token);
                InvalidatePlot();
            }, s =>
            {
                // foreach (var op in _ops)
                // {
                //     if (Application.Current != null)
                //         op.Abort();
                // }
                // _ops.Clear();
                _cts?.Cancel();
                _plotSelector.OutputPlot?.OnSessionStopped(s);
            }, s =>
            {
                // foreach (var op in _ops)
                // {
                //     if (Application.Current != null)
                //         op.Abort();
                // }
                // _ops.Clear();
                _cts?.Cancel();
                _plotSelector.OutputPlot?.OnSessionPaused(s);
            });

            _epochEndConsumer.ConsumerType = PlotEpochEndConsumerType.Buffering;
            _epochEndConsumer.BufferSize = 100;

            var p = new NavigationParameters();
            p.Add(nameof(PlotEpochEndConsumer), _epochEndConsumer);
            _rm.Regions[OutputPlotSettingsRegion].RemoveAll();
            _rm.Regions[OutputPlotSettingsRegion].RequestNavigate("PlotEpochParametersView", p);
        }

        private async void OnNavigated(NavigationContext navigationContext)
        {
            var vm = _accessor.Get<OutputPlotViewModel>();
            var parentRegion = navigationContext.Parameters[ParentRegionNavParam] as string;
            OutputPlotSettingsRegion = parentRegion + nameof(OutputPlotSettingsRegion);
            vm.BasicPlotModel.SetSettingsRegion(OutputPlotSettingsRegion);
            if (!navigationContext.Parameters.ContainsKey(EpochSubNavParam) || (bool)navigationContext.Parameters[EpochSubNavParam])
            {
                InitPlotEpochEndConsumer();
            }
            else
            {
                var set = (DataSetType)navigationContext.Parameters[SetNavParam];
                var net = navigationContext.Parameters[NetNavParam] as MLPNetwork;
                var trainingData = navigationContext.Parameters[DataNavParam] as TrainingData;
                var cts = navigationContext.Parameters[CtsNavParam] as CancellationTokenSource;

                _plotSelector.SelectPlot(net);
                await Task.Run(() =>
                {
                    _plotSelector.OutputPlot.GeneratrePlot(set, trainingData, net, vm);
                }, cts.Token);

                System.Windows.Application.Current.Dispatcher.Invoke(() => vm.PlotModel.InvalidatePlot(true),
                    DispatcherPriority.Background, cts.Token);

            }
        }
    }
}
