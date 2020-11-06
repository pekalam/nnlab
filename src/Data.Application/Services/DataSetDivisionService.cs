using Prism.Commands;
using System.Collections.Generic;
using Common.Framework;

namespace Data.Application.Services
{
    public interface IDataSetDivisionService : ITransientController
    {
        DelegateCommand<string> DivideFileDataCommand { get; set; }
        DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }
    }
}
