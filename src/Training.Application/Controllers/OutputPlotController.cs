using Common.Domain;
using Common.Framework;
using Common.Framework.Extensions;
using NNLib.Data;
using NNLib.MLP;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Prism.Events;
using Shell.Interface;
using Training.Application.Plots;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Controllers
{
    public interface IOutputPlotController : IController
    {
        Action<NavigationContext> Navigated { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IOutputPlotController, OutputPlotController>();
        }
    }

    class OutputPlotSelector
    {
        public void SelectPlot(TrainingSession session) => SelectPlot(session.Network ?? throw new NullReferenceException("Null network"));

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

    class OutputPlotController : ControllerBase<OutputPlotViewModel>, IOutputPlotController
    {
        private string _outputPlotSettingsRegion = null!;

        private PlotEpochEndConsumer? _epochEndConsumer;
        private readonly OutputPlotSelector _plotSelector = new OutputPlotSelector();
        private CancellationTokenSource? _cts;
        private readonly IRegionManager _rm;
        private readonly ModuleState _moduleState;
        private AppStateHelper _helper;

        private readonly object _pltLock = new object();

        public OutputPlotController(IRegionManager rm, ModuleState moduleState, AppStateHelper helper, IEventAggregator ea)
        {
            _rm = rm;
            _moduleState = moduleState;
            _helper = helper;

            Navigated = OnNavigated;

            ea.GetEvent<ContentRegionViewChanged>().Subscribe(() =>
            {
                // Vm!.PlotModel.Annotations.Clear();
                Vm.PlotModel.InvalidatePlot(false);
            });
        }

        protected override void VmCreated()
        {
            Vm!.IsActiveChanged += OnIsActiveChanged;

            _helper.OnTrainingDataPropertyChanged(data =>
            {
                Vm!.BasicPlotModel.Model.Series.Clear();
                _plotSelector.SelectPlot(_moduleState.ActiveSession!);
                _plotSelector.OutputPlot?.CreateForSession(_moduleState.ActiveSession!, Vm!);
                Vm!.BasicPlotModel.Model.InvalidatePlot(true);
            }, s => s switch
            {
                nameof(TrainingData.Variables) => true,
                nameof(TrainingData.Sets) => true,
                _ => false,
            });

            _helper.OnNetworkChanged(network =>
            {
                Vm!.BasicPlotModel.Model.Series.Clear();
                _plotSelector.SelectPlot(_moduleState.ActiveSession!);
                _plotSelector.OutputPlot?.CreateForSession(_moduleState.ActiveSession!, Vm!);
                Vm!.BasicPlotModel.Model.InvalidatePlot(true);
            });
        }

        private void OnIsActiveChanged(object? sender, EventArgs e)
        {
            if (!Vm!.IsActive)
            {
                _epochEndConsumer?.ForceStop();
                Vm!.IsActiveChanged -= OnIsActiveChanged;
            }
        }

        private void InvalidatePlot()
        {
            lock (_pltLock)
            {
                Vm!.PlotModel.InvalidatePlot(true);
            }
        }

        private void InitPlotEpochEndConsumer()
        {
            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState, (epochEnds, session) =>
            {
                if (_cts!.IsCancellationRequested)
                {
                    return;
                }

                lock (_pltLock)
                {
                    _plotSelector.OutputPlot?.OnEpochEnd(epochEnds, Vm!, _cts.Token);
                }

                GlobalDistributingDispatcher.CallCustom(InvalidatePlot, _epochEndConsumer!);
            }, session =>
            {
                _cts = new CancellationTokenSource();
                _plotSelector.OutputPlot?.OnSessionStarting(Vm!, session, _cts.Token);
                GlobalDistributingDispatcher.CallCustom(InvalidatePlot, _epochEndConsumer!);
            }, s =>
            {
                _cts!.Cancel();
                _plotSelector.OutputPlot?.OnSessionStopped(s);
            }, s =>
            {
                _cts!.Cancel();
                _plotSelector.OutputPlot?.OnSessionPaused(s);
            }, new PlotEpochEndConsumerOptions
            {
                DefaultConsumerType = PlotEpochEndConsumerType.Buffering,
                BufferingBufferSize = 100,
                UseOnlineSynch = true,
                FlushBufferOnConsumerTypeChange = false,
            });

            var p = new NavigationParameters();
            p.Add(nameof(PlotEpochEndConsumer), _epochEndConsumer);
            _rm.Regions[_outputPlotSettingsRegion].RemoveAll();
            _rm.Regions[_outputPlotSettingsRegion].RequestNavigate("PlotEpochParametersView", p);

            _epochEndConsumer.Initialize();
        }

        private async void OnNavigated(NavigationContext navigationContext)
        {
            var navParams = OutputPlotNavParams.FromNavParams(navigationContext.Parameters);

            _outputPlotSettingsRegion = navParams.ParentRegion + nameof(_outputPlotSettingsRegion);
            Vm!.BasicPlotModel.SetSettingsRegion?.Invoke(_outputPlotSettingsRegion);

            if (navParams.EpochEnd)
            {
                InitPlotEpochEndConsumer();
            }
            else
            {
                _plotSelector.SelectPlot(navParams.Network);
                await Task.Run(
                    () =>
                    {
                        _plotSelector.OutputPlot!.GeneratePlot(navParams.Set, navParams.Data, navParams.Network, Vm!);
                    }, navParams.Cts.Token);

                System.Windows.Application.Current.Dispatcher.Invoke(() => Vm!.PlotModel.InvalidatePlot(true),
                    DispatcherPriority.Background, navParams.Cts.Token);
            }
        }

        public Action<NavigationContext> Navigated { get; set; }
    }

    public class OutputPlotNavParams : NavigationParameters
    {
        public OutputPlotNavParams(string parentRegion, bool epochEnd, DataSetType dataSet, MLPNetwork network,
            TrainingData data, CancellationTokenSource cts)
        {
            Add(nameof(OutputPlotRecNavParams.ParentRegion), parentRegion);
            Add(nameof(OutputPlotRecNavParams.EpochEnd), epochEnd);
            Add(nameof(OutputPlotRecNavParams.Network), network);
            Add(nameof(OutputPlotRecNavParams.Data), data);
            Add(nameof(OutputPlotRecNavParams.Cts), cts);
            Add(nameof(OutputPlotRecNavParams.Set), dataSet);
        }

        public static OutputPlotRecNavParams FromNavParams(NavigationParameters navParams) =>
            new OutputPlotRecNavParams(navParams);

        public class OutputPlotRecNavParams
        {
            private readonly NavigationParameters _parameters;

            public OutputPlotRecNavParams(NavigationParameters parameters)
            {
                _parameters = parameters;
            }

            public string ParentRegion => _parameters.GetOrDefault<string>(nameof(ParentRegion));
            public bool EpochEnd => _parameters.GetOrDefault<bool>(nameof(EpochEnd), true);
            public DataSetType Set => _parameters.GetOrDefault<DataSetType>(nameof(Set));
            public MLPNetwork Network => _parameters.GetOrDefault<MLPNetwork>(nameof(Network));
            public TrainingData Data => _parameters.GetOrDefault<TrainingData>(nameof(Data));
            public CancellationTokenSource Cts => _parameters.GetOrDefault<CancellationTokenSource>(nameof(Cts));
        }
    }
}