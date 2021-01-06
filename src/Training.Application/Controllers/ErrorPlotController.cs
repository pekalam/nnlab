using Common.Framework;
using Common.Framework.Extensions;
using OxyPlot;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using Common.Domain;
using Prism.Commands;
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

        ICommand ResetCommand { get; }

        ICommand ShowValidationSeriesCommand { get; }
        ICommand HideValidationSeriesCommand { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IErrorPlotController, ErrorPlotController>();
        }
    }


    class ErrorPlotNavParams : NavigationParameters
    {
        public ErrorPlotNavParams(string parentRegion, bool epochEnd, List<DataPoint> points, List<DataPoint>? valPoints = default)
        {
            Add(nameof(ErrorPlotRecNavParams.ParentRegion), parentRegion);
            Add(nameof(ErrorPlotRecNavParams.Points), points);
            if (valPoints != null)
            {
                Add(nameof(ErrorPlotRecNavParams.ValPoints), valPoints);
            }
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
            public List<DataPoint>? ValPoints => _parameters.GetOrDefault<List<DataPoint>>(nameof(ValPoints));
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
            ResetCommand = new DelegateCommand(() =>
            {
                if (Vm!.ErrorSeries.Points.Count > 0)
                {
                    var newMin = Vm!.ErrorSeries.Points[^1].X;
                    Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
                }
            });
            ShowValidationSeriesCommand = new DelegateCommand(() =>
            {
                Vm!.ValidationSeries.IsVisible = Vm!.BasicPlotModel.Model.Legends[0].IsLegendVisible = true;
                if (_moduleState.ActiveSession!.Started)
                {
                    InvalidatePlot();
                }
                else
                {
                    Vm!.BasicPlotModel.Model.InvalidatePlot(true);
                }
            });
            HideValidationSeriesCommand = new DelegateCommand(() =>
            {
                Vm!.ValidationSeries.IsVisible = Vm!.BasicPlotModel.Model.Legends[0].IsLegendVisible = false;
                if (_moduleState.ActiveSession!.Started)
                {
                    InvalidatePlot();
                }
                else
                {
                    Vm!.BasicPlotModel.Model.InvalidatePlot(true);
                }
            });
        }

        protected override void VmCreated()
        {
            Vm!.IsActiveChanged += OnIsActiveChanged;
        }

        private void OnIsActiveChanged(object? sender, EventArgs e)
        {
            if(!Vm!.IsActive)
            {
                _epochEndConsumer?.Remove();
                Vm!.IsActiveChanged -= OnIsActiveChanged;
                _moduleState.ActiveSessionChanged -= ModuleStateOnActiveSessionChanged;
            }
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            //do not change for report view
            if (_epochEndConsumer == null) return;

            e.next.SessionReset -= NextOnSessionReset;
            e.next.SessionReset += NextOnSessionReset;

            Vm!.ErrorSeries.Points.Clear();
            Vm!.ValidationSeries.Points.Clear();

            var epochEnd = e.next.EpochEndEvents
                .Skip(e.next.EpochEndEvents.Count / EpochEndMaxPoints * EpochEndMaxPoints).ToArray();
            var points = epochEnd.Select(end => new DataPoint(end.Epoch, end.Error))
                .ToArray();

            double newMin = points.Length > 0 ? points[0].X : 0;
            Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
            Vm!.ErrorSeries.Points.AddRange(points);

            if (epochEnd.Length > 0 && epochEnd[0].ValidationError.HasValue)
            {
                var val = epochEnd.Select(end => new DataPoint(end.Epoch, end.ValidationError!.Value))
                    .ToArray();
                Vm!.ValidationSeries.Points.AddRange(val);
                Vm!.BasicPlotModel.Model.Legends[0].IsLegendVisible = Vm!.ValidationSeries.IsVisible;
            }
            else
            {
                Vm!.BasicPlotModel.Model.Legends[0].IsLegendVisible = false;
            }

            Vm!.BasicPlotModel.Model.InvalidatePlot(true);
        }

        private void NextOnSessionReset()
        {
            Vm!.ErrorSeries.Points.Clear();
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

                    bool hasValidation = endsObs[0].ValidationError.HasValue;

                    var points = endsObs.Select(end => new DataPoint(end.Epoch, end.Error)).ToArray();
                    DataPoint[] valPoints = new DataPoint[0];
                    if (hasValidation)
                    {
                        valPoints = endsObs.Select(end => new DataPoint(end.Epoch, end.ValidationError.Value)).ToArray();
                    }

                    lock (_ptsLock)
                    {
                        if (Vm!.ErrorSeries.Points.Count == 0)
                        {
                            var newMin = endsObs[0].Epoch;
                            Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
                        }

                        if (Vm!.ErrorSeries.Points.Count + endsObs.Count > EpochEndMaxPoints)
                        {
                            var newMin = endsObs[0].Epoch;
                            Vm!.ErrorSeries.Points.Clear();
                            Vm!.BasicPlotModel.Model.Axes[0].AbsoluteMinimum = newMin;
                        }

                        if (hasValidation)
                        {
                            Vm!.ValidationSeries.Points.AddRange(valPoints);
                        }

                        Vm!.ErrorSeries.Points.AddRange(points);
                    }

                    InvalidatePlot();
                }, onTrainingStopped: _ =>
                {
                    _cts!.Cancel();
                    TryPlotIfEmpty();
                }, 
                onTrainingStarting:
                _ =>
                {
                    _cts = new CancellationTokenSource();
                    Vm!.BasicPlotModel.Model.Legends[0].IsLegendVisible = Vm!.ValidationSeries.IsVisible;
                    InvalidatePlot();
                }, onTrainingPaused: _ =>
                {
                    _cts!.Cancel();
                    TryPlotIfEmpty();
                }, options: new PlotEpochEndConsumerOptions()
                {
                    DefaultConsumerType = PlotEpochEndConsumerType.Online,
                });

            var p = new NavigationParameters();
            p.Add(nameof(PlotEpochEndConsumer), _epochEndConsumer);
            p.Add("Controller", this);

            _rm.Regions[_errorPlotSettingsRegion].RemoveAll();
            _rm.Regions[_errorPlotSettingsRegion].RequestNavigate("ErrorPlotParametersView", p);

            _epochEndConsumer.Initialize();
        }

        private void TryPlotIfEmpty()
        {
            if (Vm!.ErrorSeries.Points.Count == 0)
            {
                var points = _moduleState.ActiveSession!.CurrentReport!.EpochEndEventArgs
                    .Select(end => new DataPoint(end.Epoch, end.Error)).ToArray();


                Vm!.ErrorSeries.Points.AddRange(points);
                System.Windows.Application.Current.Dispatcher.Invoke(InvalidateMethod, DispatcherPriority.Background);
            }
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
                Vm!.BasicPlotModel.DisplaySettingsRegion = false;
                Vm!.ErrorSeries.Points.Clear();
                Vm!.ErrorSeries.Points.AddRange(navParams.Points);
                if (navParams.ValPoints != null)
                {
                    Vm!.ValidationSeries.IsVisible = true;
                    Vm!.ValidationSeries.Points.Clear();
                    Vm!.ValidationSeries.Points.AddRange(navParams.ValPoints);
                    Vm!.BasicPlotModel.Model.Legends[0].IsLegendVisible = Vm!.ValidationSeries.IsVisible;
                }

                Vm!.BasicPlotModel.Model.InvalidatePlot(true);
            }
        }

        public Action<NavigationContext> Navigated { get; }
        public ICommand ResetCommand { get; }
        public ICommand ShowValidationSeriesCommand { get; }
        public ICommand HideValidationSeriesCommand { get; }
    }
}