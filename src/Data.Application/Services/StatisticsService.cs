using Common.Framework;
using Data.Application.ViewModels.DataSource.Statistics;
using NNLib.Common;
using Prism.Commands;
using System;

namespace Data.Application.Services
{
    public interface IStatisticsService : IService
    {
        DelegateCommand<DataSetType?> SelectDataSet { get; set; }
        Action<StatisticsViewModel> Created { get; set; }
    }

    internal class StatisticsService : IStatisticsService
    {
        public DelegateCommand<DataSetType?> SelectDataSet { get; set; }
        public Action<StatisticsViewModel> Created { get; set; }

        public StatisticsService(ITransientControllerBase<StatisticsService> ctrl)
        {
            ctrl.Initialize(this);
        }
    }
}
