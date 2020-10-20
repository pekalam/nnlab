using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using NNLib.Common;
using NNLib.Data;
using NNLib.MLP;
using OxyPlot;
using OxyPlot.Series;
using Prediction.Application.Services;
using Prediction.Application.ViewModels;
using Prism.Commands;
using Prism.Regions;

namespace Prediction.Application.Controllers
{
    internal static class EnumeratorExtensions
    {
        public static IEnumerable<T> IterateEnumerator<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }

    public class PredictControllerMemento
    {
        public double Interval { get; set; }
        public Matrix<double> InputMatrix { get; set; } = null!;
        public DataPoint[] DataPredictionPoints { get; set; } = null!;
        public ScatterPoint[] DataScatterPoints { get; set; } = null!;
        public ScatterPoint[] PredictionScatterPoints { get; set; } = null!;

        public DataPoint[] PredictionPoints { get; set; } = null!;

        public DataSetType SelectedPlotSetType { get; set; }
        public DataSetType[]? PlotSetTypes { get; set; } 
    }

    class PredictController : ControllerBase<PredictViewModel>, ITransientController<PredictService>
    {
        private PredictService _service = null!;
        private readonly AppState _appState;
        private readonly ModuleState _moduleState;
        private readonly AppStateHelper _helper;
        private readonly NormalizationService _normalizationService;

        private bool _initialized;

        public PredictController(IViewModelAccessor accessor, AppState appState, ModuleState moduleState, NormalizationService normalizationService) :
            base(accessor)
        {
            _appState = appState;
            _helper = new AppStateHelper(appState);
            _moduleState = moduleState;
            _normalizationService = normalizationService;
        }

        public void Initialize(PredictService service)
        {
            _service = service;
            service.PredictCommand = new DelegateCommand(Predict);
            service.Navigated = Navigated;
            service.PredictPlotCommand = new DelegateCommand<DataSetType?>(PredictPlot);
        }

        protected override void VmCreated()
        {
            Vm!.PropertyChanged += VmOnPropertyChanged;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PredictViewModel.SelectedPlotSetType))
            {
                UpdateStartEndValue(Vm!.SelectedPlotSetType);
            }
        }

        private void Navigated(NavigationContext obj)
        {
            if(_initialized) return;

            _appState.ActiveSessionChanged += (_, args) =>
            {
                if (args.prev != null)
                {
                    _moduleState.SetSessionPredictMemento(args.prev, GetMemento());
                }
            };

            _helper.OnNetworkChanged(network =>
            {
                if (_moduleState.GetSessionPredictMemento() != null)
                {
                    SetMemento(_moduleState.GetSessionPredictMemento()!);
                }
                else
                {
                    UpdateShowPlotPrediction(_appState.ActiveSession!.TrainingData!);
                    if (Vm!.ShowPlotPrediction)
                    {
                        _service.UpdateAxes(_appState.ActiveSession!.TrainingData!);
                        _service.ClearPlots();
                    }


                    var inputMatrix = Matrix<double>.Build.Dense(network.Layers[0].InputsCount, 1);
                    _service.UpdateNetworkAndMatrix(network, _appState.ActiveSession!.TrainingData!, inputMatrix);
                }

                _appState.ActiveSession!.NetworkStructureChanged -= ActiveSessionOnNetworkStructureChanged;
                _appState.ActiveSession!.NetworkStructureChanged += ActiveSessionOnNetworkStructureChanged;
            });


            _helper.OnTrainingDataPropertyChanged(data =>
            {
                UpdateShowPlotPrediction(data);

                if (Vm!.ShowPlotPrediction)
                {
                    _service.UpdateAxes(data);
                    Vm!.PlotSetTypes = data.SetTypes;
                    Vm!.SelectedPlotSetType = DataSetType.Training;
                    UpdateStartEndValue(DataSetType.Training);
                }
            }, s => s switch
            {
                nameof(TrainingData.Sets) => true,
                nameof(TrainingData.Variables) => true,
                _ => false,
            });

            _initialized = true;
        }

        public void SetMemento(PredictControllerMemento memento)
        {
            Vm!.Interval = memento.Interval;
            Vm!.PlotSetTypes = memento.PlotSetTypes;
            Vm!.SelectedPlotSetType = memento.SelectedPlotSetType;
            _service.UpdateNetworkAndMatrix(_appState.ActiveSession!.Network!, _appState.ActiveSession.TrainingData!,
                memento.InputMatrix);
            _service.UpdatePlots(memento.DataScatterPoints, memento.DataPredictionPoints, memento.PredictionPoints, memento.PredictionScatterPoints);
            UpdateStartEndValue(memento.SelectedPlotSetType);
        }

        private void ActiveSessionOnNetworkStructureChanged(MLPNetwork network)
        {
            _service.UpdateNetworkAndMatrix(_appState.ActiveSession!.Network!, _appState.ActiveSession!.TrainingData!,
                Matrix<double>.Build.Dense(network.Layers[0].InputsCount, 1));
            UpdateShowPlotPrediction(_appState.ActiveSession!.TrainingData!);
        }


        private void UpdateShowPlotPrediction(TrainingData data)
        {
            if (data.Sets.TrainingSet.Input[0].RowCount == 1 &&
                data.Sets.TrainingSet.Target[0].RowCount == 1)
            {
                Vm!.ShowPlotPrediction = true;
                UpdateStartEndValue(DataSetType.Training);
                Vm!.PlotSetTypes = data.SetTypes;
                Vm!.SelectedPlotSetType = DataSetType.Training;
            }
            else
            {
                Vm!.ShowPlotPrediction = false;
            }
        }


        private void UpdateStartEndValue(DataSetType setType)
        {
            var data = _appState.ActiveSession!.TrainingData!.GetOriginalSet(setType)!;
            Vm!.StartValue = data.Input.GetEnumerator().IterateEnumerator().Min(m => m[0, 0]);
            Vm!.EndValue = data.Input.GetEnumerator().IterateEnumerator().Max(m => m[0, 0]);
        }

        private async void PredictPlot(DataSetType? setType)
        {
            Debug.Assert(setType != null, nameof(setType) + " != null");

            var orgSet = _appState.ActiveSession!.TrainingData!.GetOriginalSet(setType.Value);
            var set = _appState.ActiveSession!.TrainingData!.GetSet(setType.Value);
            Debug.Assert(orgSet != null, nameof(orgSet) + " != null");
            Debug.Assert(set != null, nameof(set) + " != null");

            var network = _appState.ActiveSession!.Network!;
            var dataScatter = new ScatterPoint[orgSet.Input.Count];
            var dataPredLine = new DataPoint[orgSet.Input.Count];
            var predLine = new List<DataPoint>(1000);
            var predScatter = new List<ScatterPoint>(1000);
            var setStart = orgSet.Input.GetEnumerator().IterateEnumerator().Min(m => m[0, 0]);
            var setEnd = orgSet.Input.GetEnumerator().IterateEnumerator().Max(m => m[0, 0]);


            for (int i = 0; i < orgSet.Input.Count; i++)
            {
                dataScatter[i] = new ScatterPoint(orgSet.Input[i][0, 0], orgSet.Target[i][0, 0]);
            }

            await Task.Run(() =>
            {
                for (int i = 0; i < orgSet.Input.Count; i++)
                {
                    network.CalculateOutput(set.Input[i]);
                    dataPredLine[i] = new DataPoint(orgSet.Input[i].At(0, 0), network.Output!.At(0, 0));
                }

                var start = Vm!.StartValue;

                if (Vm!.StartValue != setStart || Vm!.EndValue != setEnd)
                {
                    while (start <= Vm!.EndValue)
                    {
                        var x = _normalizationService.ToNetworkDataNormalization(Matrix<double>.Build.Dense(1, 1, start));
                        network.CalculateOutput(x);
                        predLine.Add(new DataPoint(start, network.Output![0, 0]));
                        predScatter.Add(new ScatterPoint(start, network.Output![0, 0]));

                        start += Vm!.Interval;
                    }
                }

            });

            _service.UpdatePlots(dataScatter, 
                dataPredLine.OrderBy(p => p.X).ToArray(),
                predLine.OrderBy(p => p.X).ToArray(), 
                predScatter.ToArray());
        }

        private void Predict()
        {
            var network = _appState.ActiveSession!.Network!;
            var inputMatrix = Vm!.InputMatrixVm.Controller.AssignedMatrix!;
            var inputNormalized = _normalizationService.ToNetworkDataNormalization(inputMatrix);

            network.CalculateOutput(inputNormalized);
            _service.UpdateMatrix(network.Output!, _appState.ActiveSession!.TrainingData!, inputMatrix);
        }


        public PredictControllerMemento GetMemento()
        {
            return new PredictControllerMemento()
            {
                Interval = Vm!.Interval,
                InputMatrix = Vm!.InputMatrixVm.Controller.AssignedMatrix!,
                DataPredictionPoints = Vm!.DataPredictionLineSeries.Points.ToArray(),
                DataScatterPoints = Vm!.DataScatterSeries.Points.ToArray(),
                PredictionPoints = Vm!.PredictionLineSeries.Points.ToArray(),
                PredictionScatterPoints = Vm!.PredictionScatterSeries.Points.ToArray(),
                SelectedPlotSetType = Vm!.SelectedPlotSetType,
                PlotSetTypes = Vm!.PlotSetTypes,
            };
        }
    }
}