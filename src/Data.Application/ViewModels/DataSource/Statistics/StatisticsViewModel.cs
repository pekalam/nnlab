using Common.Framework;
using Data.Application.Controllers.DataSource;
using Data.Application.Services;
using NNLib.Common;
using NNLib.Data;
using Unity;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class StatisticsViewModel : ViewModelBase<StatisticsViewModel>
    {
        private DataSetType[]? _dataSetTypes;
        private DataSetType _selectedDataSetType;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public StatisticsViewModel() { }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        
        [InjectionConstructor]
        public StatisticsViewModel(IStatisticsService service)
        {
            Service = service;
        }

        public IStatisticsService Service { get; }

        public HistogramViewModel HistogramVm { get; } = new HistogramViewModel();
        public VariablesPlotViewModel VariablesPlotVm { get; } = new VariablesPlotViewModel();



        public DataSetType[]? DataSetTypes
        {
            get => _dataSetTypes;
            set => SetProperty(ref _dataSetTypes, value);
        }


        public DataSetType SelectedDataSetType
        {
            get => _selectedDataSetType;
            set
            {
                _selectedDataSetType = value;
                RaisePropertyChanged();
            }
        }
    }
}
