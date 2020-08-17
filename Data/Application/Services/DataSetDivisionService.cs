using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using NNLib.Common;

namespace Data.Application.Services
{
    public interface IDataSetDivisionService
    {
        DelegateCommand<string> DivideFileDataCommand { get; set; }
        DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }
    }

    internal class DataSetDivisionService : IDataSetDivisionService
    {
        public DelegateCommand<string> DivideFileDataCommand { get; set; }
        public DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }
    }
}
