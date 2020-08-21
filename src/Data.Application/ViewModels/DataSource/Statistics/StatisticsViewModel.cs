using System.Text;
using Common.Framework;
using Data.Application.Services;
using Infrastructure;
using NNLib.Common;
using Serilog;
using Unity;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class StatisticsViewModel : ViewModelBase<StatisticsViewModel>
    {
        private DataSetType[] _dataSetTypes;

        public StatisticsViewModel() { }

        [InjectionConstructor]
        public StatisticsViewModel(IStatisticsService service)
        {
            Service = service;
            service.Created?.Invoke(this);
        }

        public IStatisticsService Service { get; }

        public HistogramViewModel HistogramVm { get; } = new HistogramViewModel();
        public VariablesPlotViewModel VariablesPlotVm { get; } = new VariablesPlotViewModel();



        public DataSetType[] DataSetTypes
        {
            get => _dataSetTypes;
            set => SetProperty(ref _dataSetTypes, value);
        }
    }
}
