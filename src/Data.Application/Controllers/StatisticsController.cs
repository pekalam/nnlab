using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using NNLib.Data;
using Prism.Ioc;
using System.Collections.Generic;
using System.ComponentModel;

namespace Data.Application.Controllers.DataSource
{
    public interface IStatisticsController : ITransientController
    {
        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IStatisticsController, StatisticsController>();
        }
    }
    internal class StatisticsController : ControllerBase<StatisticsViewModel>,IStatisticsController
    {
        private HistogramController _histogramCtrl = null!;
        private VariablesPlotController _variablesPlotCtrl = null!;
        private readonly AppState _appState;
        private readonly AppStateHelper _helper;

        public StatisticsController(AppState appState)
        {
            _appState = appState;
            _helper = new AppStateHelper(appState);
        }

        protected override void VmCreated()
        {
            Vm!.VariablesPlotVm.PropertyChanged += VariablesPlotVmOnPropertyChanged;
            Vm!.PropertyChanged += VmOnPropertyChanged;
            _histogramCtrl = new HistogramController(Vm!.HistogramVm, _appState);
            _variablesPlotCtrl = new VariablesPlotController(Vm!.VariablesPlotVm);

            _helper.OnTrainingDataInSession(data =>
            {
                if (data != null) SetTrainingData();
            });

            _helper.OnTrainingDataPropertyChanged(data =>
            {
                Vm!.DataSetTypes = data.SetTypes;
                Vm!.SelectedDataSetType = DataSetType.Training;
            }, s => s switch
            {
                nameof(TrainingData.Variables) => true,
                nameof(TrainingData.Sets) => true,
                nameof(TrainingData.NormalizationMethod) => true,
                _ => false,
            });

            SetTrainingData();
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StatisticsViewModel.SelectedDataSetType))
            {
                _variablesPlotCtrl.Plot(_appState.ActiveSession!.TrainingData!, Vm!.SelectedDataSetType);
            }
        }

        private void VariablesPlotVmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VariablesPlotViewModel.SelectedVariablePlotType))
            {
                _variablesPlotCtrl.Plot(_appState.ActiveSession!.TrainingData!, Vm!.SelectedDataSetType);
            }
        }


        private void SetTrainingData()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;

            var setTypes = new List<DataSetType>() {DataSetType.Training};
            if (trainingData.Sets.TestSet != null) setTypes.Add(DataSetType.Test);
            if (trainingData.Sets.ValidationSet != null) setTypes.Add(DataSetType.Validation);
            Vm!.DataSetTypes = setTypes.ToArray();
            Vm!.SelectedDataSetType = DataSetType.Training;

            _variablesPlotCtrl.Plot(_appState.ActiveSession!.TrainingData!, Vm!.SelectedDataSetType);
        }
    }
}