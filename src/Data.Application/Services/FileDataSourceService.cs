using Prism.Commands;
using System;
using Prism.Regions;

namespace Data.Application.Services
{
    public interface IFileDataSourceService
    {
        Action Initialized { get; set; }
        DelegateCommand SelectVariablesCommand { get; set; }
        DelegateCommand DivideDatasetCommand { get; set; }
        Action<NavigationContext> Navigated { get; }
    }

    internal class FileDataSourceService : IFileDataSourceService
    {
        public Action Initialized { get; set; }
        public DelegateCommand SelectVariablesCommand { get; set; }
        public DelegateCommand DivideDatasetCommand { get; set; }
        public Action<NavigationContext> Navigated { get; set; }
    }
}
