using Prism.Commands;
using System;
using Common.Framework;
using Prism.Regions;

namespace Data.Application.Services
{
    public interface IFileDataSourceService : ITransientController
    {
        Action Initialized { get; set; }
        DelegateCommand SelectVariablesCommand { get; set; }
        DelegateCommand DivideDatasetCommand { get; set; }
        Action<NavigationContext> Navigated { get; }
    }
}
