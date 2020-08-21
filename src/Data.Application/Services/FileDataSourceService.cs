using System;
using System.Collections.Generic;
using System.Text;
using Prism.Commands;

namespace Data.Application.Services
{
    public interface IFileDataSourceService
    {
        Action Initialized { get; set; }
        DelegateCommand SelectVariablesCommand { get; set; }
        DelegateCommand DivideDatasetCommand { get; set; }
    }

    internal class FileDataSourceService : IFileDataSourceService
    {
        public Action Initialized { get; set; }
        public DelegateCommand SelectVariablesCommand { get; set; }
        public DelegateCommand DivideDatasetCommand { get; set; }
    }
}
