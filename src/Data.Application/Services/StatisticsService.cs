using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Data.Application.ViewModels.DataSource.Statistics;
using Infrastructure;
using NNLib.Common;
using Prism.Commands;

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
