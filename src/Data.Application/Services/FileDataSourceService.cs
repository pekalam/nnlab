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
        public Action Initialized { get; set; } = null!;
        public DelegateCommand SelectVariablesCommand { get; set; } = null!;
        public DelegateCommand DivideDatasetCommand { get; set; } = null!;
        public Action<NavigationContext> Navigated { get; set; } = null!;
    }
}
