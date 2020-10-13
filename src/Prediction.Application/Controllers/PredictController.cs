﻿using System;
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
        public Matrix<double> InputMatrix { get; set; }
        public DataPoint[] DataPredictionPoints { get; set; }
        public ScatterPoint[] DataScatterPoints { get; set; }
        public ScatterPoint[] PredictionScatterPoints { get; set; }

        public DataPoint[] PredictionPoints { get; set; }

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
                    var inputMatrix = Matrix<double>.Build.Dense(network.Layers[0].InputsCount, 1);
                    _service.UpdateNetworkAndMatrix(_appState.ActiveSession!.Network!,
                        _appState.ActiveSession.TrainingData!, inputMatrix);
                    _service.ClearPlots();
                }

                _appState.ActiveSession!.NetworkStructureChanged -= ActiveSessionOnNetworkStructureChanged;
                _appState.ActiveSession!.NetworkStructureChanged += ActiveSessionOnNetworkStructureChanged;
            });

            _helper.OnTrainingDataChanged(data =>
            {
                UpdateShowPlotPrediction(data);
                _service.UpdateAxes(data);

                _appState.ActiveSession!.TrainingDataUpdated -= SessionOnTrainingDataUpdated;
                _appState.ActiveSession!.TrainingDataUpdated += SessionOnTrainingDataUpdated;
            });

            _initialized = true;
        }

        public void SetMemento(PredictControllerMemento memento)
        {
            Vm!.Interval = memento.Interval;
            _service.UpdateNetworkAndMatrix(_appState.ActiveSession!.Network!, _appState.ActiveSession.TrainingData!,
                memento.InputMatrix);
            _service.UpdatePlots(memento.DataScatterPoints, memento.DataPredictionPoints, memento.PredictionPoints, memento.PredictionScatterPoints);
            _service.UpdateAxes(_appState.ActiveSession.TrainingData!);
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
                Vm!.StartValue = data.Sets.TrainingSet.Input.GetEnumerator().IterateEnumerator().Min(m => m[0, 0]);
                Vm!.EndValue = data.Sets.TrainingSet.Input.GetEnumerator().IterateEnumerator().Max(m => m[0, 0]);
                Vm!.PlotSetTypes = data.SetTypes;
            }
            else
            {
                Vm!.ShowPlotPrediction = false;
            }
        }

        private void SessionOnTrainingDataUpdated(TrainingData data)
        {
            UpdateShowPlotPrediction(data);
            _service.UpdateAxes(data);
            Vm!.PlotSetTypes = data.SetTypes;
        }

        private async void PredictPlot(DataSetType? setType)
        {
            Debug.Assert(setType != null, nameof(setType) + " != null");

            var set = _appState.ActiveSession!.TrainingData!.GetSet(setType.Value);
            Debug.Assert(set != null, nameof(set) + " != null");

            var network = _appState.ActiveSession!.Network!;
            var dataScatter = new ScatterPoint[set.Input.Count];
            var dataPredLine = new DataPoint[set.Input.Count];
            var predLine = new List<DataPoint>(1000);
            var predScatter = new List<ScatterPoint>(1000);
            var setStart = set.Input.GetEnumerator().IterateEnumerator().Min(m => m[0, 0]);
            var setEnd = set.Input.GetEnumerator().IterateEnumerator().Max(m => m[0, 0]);


            for (int i = 0; i < set.Input.Count; i++)
            {
                dataScatter[i] = new ScatterPoint(set.Input[i][0, 0], set.Target[i][0, 0]);
            }

            await Task.Run(() =>
            {
                for (int i = 0; i < set.Input.Count; i++)
                {
                    network.CalculateOutput(set.Input[i]);
                    dataPredLine[i] = new DataPoint(set.Input[i].At(0, 0), network.Output!.At(0, 0));
                }

                var x = Matrix<double>.Build.Dense(1, 1, Vm!.StartValue);

                if (Vm!.StartValue != setStart || Vm!.EndValue != setEnd)
                {
                    while (x.At(0, 0) <= Vm!.EndValue)
                    {
                        network.CalculateOutput(x);
                        predLine.Add(new DataPoint(x.At(0, 0), network.Output![0, 0]));
                        predScatter.Add(new ScatterPoint(x.At(0, 0), network.Output![0, 0]));

                        x += Vm!.Interval;
                    }
                }

            });

            _service.UpdatePlots(dataScatter, dataPredLine, predLine.ToArray(), predScatter.ToArray());
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
            };
        }
    }
}