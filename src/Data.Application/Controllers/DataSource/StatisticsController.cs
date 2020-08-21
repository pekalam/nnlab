using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.Statistics;
using Infrastructure.Domain;
using NNLib.Common;
using Prism.Commands;
using System.Collections.Generic;
using System.ComponentModel;

namespace Data.Application.Controllers.DataSource
{
    internal class StatisticsController : ITransientControllerBase<StatisticsService>
    {
        private HistogramController _histogramCtrl;
        private VariablesPlotController _variablesPlotCtrl;
        private AppState _appState;

        public StatisticsController(AppState appState)
        {
            _appState = appState;

            _appState.SessionManager.ActiveSession.PropertyChanged += ActiveSessionOnPropertyChanged;
        }

        private void ActiveSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Session.TrainingData):
                    SetTrainingData();
                    break;
            }
        }


        private void SetTrainingData()
        {
            var trainingData = _appState.SessionManager.ActiveSession.TrainingData;

            var setTypes = new List<DataSetType>() {DataSetType.Training};
            if (trainingData.Sets.TestSet != null) setTypes.Add(DataSetType.Test);
            if (trainingData.Sets.ValidationSet != null) setTypes.Add(DataSetType.Validation);
            StatisticsViewModel.Instance.DataSetTypes = setTypes.ToArray();

            _variablesPlotCtrl.Plot(trainingData, DataSetType.Training);
        }


        public void Initialize(StatisticsService service)
        {
            service.Created = vm =>
            {
                _histogramCtrl = new HistogramController(vm.HistogramVm);
                _variablesPlotCtrl = new VariablesPlotController(vm.VariablesPlotVm);

                SetTrainingData();
                service.SelectDataSet = new DelegateCommand<DataSetType?>(SelectDataSet);
            };
        }

        private void SelectDataSet(DataSetType? set)
        {
            _variablesPlotCtrl.Plot(_appState.SessionManager.ActiveSession.TrainingData, set.Value);
        }
    }
}