﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Approximation.Application.ViewModels;
using Common.Domain;
using Common.Framework;
using MathNet.Numerics.LinearAlgebra;
using NNLib.Data;
using NNLib.MLP;
using OxyPlot;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;
using IController = Common.Framework.IController;

namespace Approximation.Application.Controllers
{
    public interface IApproximationController : IController
    {
        DelegateCommand CalculateOutputCommand { get; }
        Action<NavigationContext>? Navigated { get; set; }

        DelegateCommand<DataSetType?> PlotOutputCommand { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IApproximationController, ApproximationController>();
        }
    }

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

    public class ApproximationControllerMemento
    {
        public double Interval { get; set; }
        public Matrix<double> InputMatrix { get; set; } = null!;
        public DataPoint[] DataPredictionPoints { get; set; } = null!;
        public ScatterPoint[] DataScatterPoints { get; set; } = null!;
        public ScatterPoint[] DataPredScatterPoints { get; set; } = null!;
        public ScatterPoint[] PredictionScatterPoints { get; set; } = null!;

        public DataPoint[] PredictionPoints { get; set; } = null!;

        public DataSetType SelectedPlotSetType { get; set; }
        public DataSetType[]? PlotSetTypes { get; set; }
    }

    class ApproximationController : ControllerBase<ApproximationViewModel>, IApproximationController
    {
        private readonly AppState _appState;
        private readonly ModuleState _moduleState;
        private readonly AppStateHelper _helper;
        private readonly IEventAggregator _ea;

        public ApproximationController(AppState appState, ModuleState moduleState, IEventAggregator ea)
        {
            _appState = appState;
            _helper = new AppStateHelper(appState);
            _moduleState = moduleState;
            _ea = ea;

            CalculateOutputCommand = new DelegateCommand(Predict);
            Navigated = NavigatedAction;
            PlotOutputCommand = new DelegateCommand<DataSetType?>(PredictPlot);
        }

        protected override void VmCreated()
        {
            Vm!.PropertyChanged += VmOnPropertyChanged;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ApproximationViewModel.SelectedPlotSetType))
            {
                Vm!.ClearPlots();
                UpdateStartEndValue(Vm!.SelectedPlotSetType);
            }
        }

        private void NavigatedAction(NavigationContext obj)
        {
            _appState.ActiveSessionChanged += (_, args) =>
            {
                if (args.prev != null)
                {
                    _moduleState.SetSessionPredictMemento(args.prev, GetMemento());
                }
            };

            _helper.OnNetworkChanged(network =>
            {
                UpdateUi();

                _appState.ActiveSession!.NetworkStructureChanged -= ActiveSessionOnNetworkStructureChanged;
                _appState.ActiveSession!.NetworkStructureChanged += ActiveSessionOnNetworkStructureChanged;
            });

            _helper.OnTrainingDataPropertyChanged(data =>
            {
                UpdateShowPlotPrediction(data);

                if (Vm!.ShowPlotPrediction)
                {
                    Vm!.ClearPlots();
                    Vm!.UpdateAxes(data);
                    UpdateStartEndValue(DataSetType.Training);
                }
            }, s => s switch
            {
                nameof(TrainingData.Sets) => true,
                nameof(TrainingData.NormalizationMethod) => true,
                nameof(TrainingData.Variables) => true,
                _ => false,
            });

            Navigated = null;
        }

        private void UpdateUi()
        {
            if (_appState.ActiveSession!.TrainingData == null) return;
            if (_appState.ActiveSession!.Network == null) return;

            if (_moduleState.GetSessionPredictMemento() != null)
            {
                SetMemento(_moduleState.GetSessionPredictMemento()!);
            }
            else
            {
                UpdateShowPlotPrediction(_appState.ActiveSession!.TrainingData!);
                if (Vm!.ShowPlotPrediction)
                {
                    Vm!.UpdateAxes(_appState.ActiveSession!.TrainingData!);
                    Vm!.ClearPlots();
                }

                if (Vm!.EndValue - Vm!.EndValue != 0d)
                {
                    Vm!.Interval = (Vm!.EndValue - Vm!.StartValue) /
                                   _appState.ActiveSession.TrainingData.Sets.TrainingSet.Input.Count;
                }


                var inputMatrix = Matrix<double>.Build.Dense(_appState.ActiveSession.Network.Layers[0].InputsCount, 1);
                Vm!.UpdateNetworkAndMatrix(_appState.ActiveSession.Network, _appState.ActiveSession!.TrainingData!,
                    inputMatrix);
            }
            Vm!.WarningMessage = null;
        }

        private void ActiveSessionOnNetworkStructureChanged(MLPNetwork network)
        {
            Vm!.UpdateNetworkAndMatrix(_appState.ActiveSession!.Network!, _appState.ActiveSession!.TrainingData!,
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

            if (Vm!.StartValue == Vm!.EndValue)
            {
                Vm!.EndValue = Vm!.StartValue + 1;
            }
        }

        private async void PredictPlot(DataSetType? setType)
        {
            Debug.Assert(setType != null, nameof(setType) + " != null");

            var orgSet = _appState.ActiveSession!.TrainingData!.GetOriginalSet(setType.Value);
            var set = _appState.ActiveSession!.TrainingData!.GetSet(setType.Value);
            Debug.Assert(orgSet != null, nameof(orgSet) + " != null");
            Debug.Assert(set != null, nameof(set) + " != null");

            if (set.Input.Count > 5000)
            {
                _ea.GetEvent<ShowGlobalLoading>().Publish("Generating plot...");
            }


            var network = _appState.ActiveSession!.Network!;
            var dataScatter = new ScatterPoint[orgSet.Input.Count];
            var dataPredScatter = new ScatterPoint[orgSet.Input.Count];
            var dataPredLine = new DataPoint[orgSet.Input.Count];
            var predLine = new List<DataPoint>(1000);
            var predScatter = new List<ScatterPoint>(1000);


            await Task.Run(() =>
            {
                var start = Vm!.StartValue;
                int total = (int) Math.Round((Vm!.EndValue - start) / Vm!.Interval, MidpointRounding.AwayFromZero) + 1;

                if (total > 15_000)
                {
                    Vm!.WarningMessage = "Reached max number of points";
                    _ea.GetEvent<ShowGlobalLoading>().Publish("Generating plot...");
                }
                else
                {
                    Vm!.WarningMessage = null;
                }

                for (int i = 0; i < orgSet.Input.Count; i++)
                {
                    dataScatter[i] = new ScatterPoint(orgSet.Input[i][0, 0], orgSet.Target[i][0, 0]);
                }

                for (int i = 0; i < set.Input.Count; i++)
                {
                    network.CalculateOutput(set.Input[i]);
                    dataPredLine[i] = new DataPoint(orgSet.Input[i].At(0, 0), network.Output!.At(0, 0));
                    dataPredScatter[i] = new ScatterPoint(orgSet.Input[i].At(0, 0), network.Output!.At(0, 0));
                }

                if (total > 15_000)
                {
                    Vm!.Interval = (Vm!.EndValue - start) / 15_000;
                    total = (int) Math.Round((Vm!.EndValue - start) / Vm!.Interval, MidpointRounding.AwayFromZero) + 1;
                }

                while (total-- > 0)
                {
                    var x = Matrix<double>.Build.Dense(1, 1, start);
                    x = _appState.ActiveSession!.TrainingData!.Normalization?.Transform(x) ?? x;

                    network.CalculateOutput(x);
                    predLine.Add(new DataPoint(start, network.Output![0, 0]));
                    predScatter.Add(new ScatterPoint(start, network.Output![0, 0]));

                    start += Vm!.Interval;
                }
            });

            Vm!.UpdatePlots(_appState.ActiveSession!.TrainingData!, dataScatter, dataPredScatter,
                dataPredLine.OrderBy(p => p.X).ToArray(),
                predLine.OrderBy(p => p.X).ToArray(),
                predScatter.ToArray());


            _ea.GetEvent<HideGlobalLoading>().Publish();
        }

        private void Predict()
        {
            var network = _appState.ActiveSession!.Network!;
            var inputMatrix = Vm!.InputMatrixVm.Controller.AssignedMatrix!;
            var inputNormalized =
                _appState.ActiveSession!.TrainingData!.Normalization?.Transform(inputMatrix) ?? inputMatrix;

            network.CalculateOutput(inputNormalized);
            Vm!.UpdateMatrix(network.Output!, _appState.ActiveSession!.TrainingData!, inputMatrix);
        }

        public void SetMemento(ApproximationControllerMemento memento)
        {
            if (memento.InputMatrix.RowCount != _appState.ActiveSession!.Network!.BaseLayers[0].InputsCount)
            {
                memento.InputMatrix = Matrix<double>.Build.Dense(_appState.ActiveSession!.Network!.Layers[0].InputsCount, 1);
            }


            UpdateShowPlotPrediction(_appState.ActiveSession!.TrainingData!);
            if (Vm!.ShowPlotPrediction)
            {
                Vm!.SelectedPlotSetType = memento.SelectedPlotSetType;
                Vm!.UpdateAxes(_appState.ActiveSession!.TrainingData!);
                Vm!.UpdatePlots(_appState.ActiveSession!.TrainingData!, memento.DataScatterPoints, memento.DataPredScatterPoints,
                    memento.DataPredictionPoints, memento.PredictionPoints, memento.PredictionScatterPoints);
            }

            Vm!.Interval = memento.Interval;
            Vm!.UpdateNetworkAndMatrix(_appState.ActiveSession!.Network!, _appState.ActiveSession.TrainingData!,
                memento.InputMatrix);

            UpdateStartEndValue(memento.SelectedPlotSetType);
        }

        public ApproximationControllerMemento GetMemento()
        {
            return new ApproximationControllerMemento()
            {
                Interval = Vm!.Interval,
                InputMatrix = Vm!.InputMatrixVm.Controller.AssignedMatrix!,
                DataPredictionPoints = Vm!.DataPredictionLineSeries.Points.ToArray(),
                DataScatterPoints = Vm!.DataScatterSeries.Points.ToArray(),
                PredictionPoints = Vm!.PredictionLineSeries.Points.ToArray(),
                PredictionScatterPoints = Vm!.PredictionScatterSeries.Points.ToArray(),
                SelectedPlotSetType = Vm!.SelectedPlotSetType,
                PlotSetTypes = Vm!.PlotSetTypes,
                DataPredScatterPoints = Vm!.DataPredictionScatter.Points.ToArray(),
            };
        }

        public DelegateCommand CalculateOutputCommand { get; }
        public Action<NavigationContext>? Navigated { get; set; }
        public DelegateCommand<DataSetType?> PlotOutputCommand { get; }
    }
}