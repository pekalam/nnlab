using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using NNLib.Data;
using Prism.Ioc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using HistogramViewModel = Data.Application.ViewModels.HistogramViewModel;

namespace Data.Application.Controllers.DataSource
{
    public interface IStatisticsController : IController
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
            _histogramCtrl = new HistogramController(Vm!.HistogramVm, _appState);
            _variablesPlotCtrl = new VariablesPlotController(Vm!.VariablesPlotVm);

            _helper.OnTrainingDataInSession(data =>
            {
                if (data != null && data.Source != TrainingDataSource.Memory) SetTrainingData();
            });

            _helper.OnTrainingDataPropertyChanged(data =>
            {
                if(data.Source == TrainingDataSource.Memory) return;

                Vm!.DataSetTypes = data.SetTypes;
                Vm!.SelectedDataSetType = DataSetType.Training;

                _histogramCtrl.PlotColumnDataOnHistogram(Vm!.HistogramVm.SelectedVariable!, Vm!.SelectedDataSetType);
            }, s => s switch
            {
                nameof(TrainingData.Variables) => true,
                nameof(TrainingData.Sets) => true,
                nameof(TrainingData.NormalizationMethod) => true,
                _ => false,
            });
        }
        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StatisticsViewModel.SelectedDataSetType))
            {
                _variablesPlotCtrl.Plot(_appState.ActiveSession!.TrainingData!, Vm!.SelectedDataSetType);
                _histogramCtrl.PlotColumnDataOnHistogram(Vm!.HistogramVm.SelectedVariable!, Vm!.SelectedDataSetType);
            }
        }

        private void HistogramVmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(HistogramViewModel.SelectedVariable):
                    _histogramCtrl.PlotColumnDataOnHistogram(Vm!.HistogramVm.SelectedVariable!, Vm!.SelectedDataSetType);
                    break;
                case nameof(HistogramViewModel.BinWidth):
                    _histogramCtrl.UpdateBinWidth(Vm!.SelectedDataSetType);
                    break;
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
            Vm!.VariablesPlotVm.PropertyChanged -= VariablesPlotVmOnPropertyChanged;
            Vm!.HistogramVm.PropertyChanged -= HistogramVmOnPropertyChanged;
            Vm!.PropertyChanged -= VmOnPropertyChanged;


            var trainingData = _appState.ActiveSession!.TrainingData!;

            var setTypes = new List<DataSetType>() {DataSetType.Training};
            if (trainingData.Sets.TestSet != null) setTypes.Add(DataSetType.Test);
            if (trainingData.Sets.ValidationSet != null) setTypes.Add(DataSetType.Validation);
            Vm!.DataSetTypes = setTypes.ToArray();
            Vm!.SelectedDataSetType = DataSetType.Training;
            Vm!.HistogramVm.Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames).ToArray();
            Vm!.HistogramVm.SelectedVariable = Vm!.HistogramVm.Variables[0];

            _variablesPlotCtrl.Plot(_appState.ActiveSession!.TrainingData!, Vm!.SelectedDataSetType);
            _histogramCtrl.PlotColumnDataOnHistogram(Vm!.HistogramVm.SelectedVariable, Vm!.SelectedDataSetType);


            Vm!.VariablesPlotVm.PropertyChanged += VariablesPlotVmOnPropertyChanged;
            Vm!.HistogramVm.PropertyChanged += HistogramVmOnPropertyChanged;
            Vm!.PropertyChanged += VmOnPropertyChanged;
        }
    }
}