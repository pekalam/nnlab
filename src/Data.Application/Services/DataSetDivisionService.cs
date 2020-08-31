using Prism.Commands;
using System.Collections.Generic;

namespace Data.Application.Services
{
    public interface IDataSetDivisionService
    {
        DelegateCommand<string> DivideFileDataCommand { get; set; }
        DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }
    }

    internal class DataSetDivisionService : IDataSetDivisionService
    {
        public DelegateCommand<string> DivideFileDataCommand { get; set; } = null!;
        public DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }= null!;
    }
}
