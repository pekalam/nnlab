using System;
using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.Statistics;
using NNLib.Common;
using Prism.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using Common.Domain;
using Data.Application.Controllers.DataSource;
using Prism.Ioc;

namespace Data.Application.Services
{
    public interface IStatisticsService : ITransientController
    {
        DelegateCommand<DataSetType?> SelectDataSet { get; set; }
        Action<StatisticsViewModel> Created { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IStatisticsService, StatisticsController>();
        }
    }
}

namespace Data.Application.Controllers.DataSource
{
    internal class StatisticsController : IStatisticsService
    {
        private HistogramController _histogramCtrl = null!;
        private VariablesPlotController _variablesPlotCtrl = null!;
        private readonly AppState _appState;

        public StatisticsController(AppState appState)
        {
            _appState = appState;

            _appState.ActiveSession!.PropertyChanged += ActiveSessionOnPropertyChanged;
            _appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;

            Created = vm =>
            {
                _histogramCtrl = new HistogramController(vm.HistogramVm);
                _variablesPlotCtrl = new VariablesPlotController(vm.VariablesPlotVm);

                SetTrainingData();
                SelectDataSet = new DelegateCommand<DataSetType?>(SelectDataSetExecute);
            };
        }

        public DelegateCommand<DataSetType?> SelectDataSet { get; set; } = null!;
        public Action<StatisticsViewModel> Created { get; set; }

        private void ActiveSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Session.TrainingData):
                    SetTrainingData();
                    break;
            }
        }

        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) e)
        {
            if (e.next.TrainingData != null)
            {
                SetTrainingData();
            }
            e.next.PropertyChanged -= ActiveSessionOnPropertyChanged;
            e.next.PropertyChanged += ActiveSessionOnPropertyChanged;
        }


        private void SetTrainingData()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;

            var setTypes = new List<DataSetType>() {DataSetType.Training};
            if (trainingData.Sets.TestSet != null) setTypes.Add(DataSetType.Test);
            if (trainingData.Sets.ValidationSet != null) setTypes.Add(DataSetType.Validation);
            StatisticsViewModel.Instance!.DataSetTypes = setTypes.ToArray();

            _variablesPlotCtrl.Plot(trainingData, DataSetType.Training);

            trainingData.PropertyChanged -= TrainingDataOnPropertyChanged;
            trainingData.PropertyChanged += TrainingDataOnPropertyChanged;
        }

        private void TrainingDataOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(TrainingData.Variables):
                    _variablesPlotCtrl.Plot((sender as TrainingData)!, DataSetType.Training);
                    break;
                case nameof(TrainingData.Sets):
                    _variablesPlotCtrl.Plot((sender as TrainingData)!, DataSetType.Training);
                    break;

            }
        }

        private void SelectDataSetExecute(DataSetType? set)
        {
            _variablesPlotCtrl.Plot(_appState.ActiveSession!.TrainingData!, set!.Value);
        }
    }
}