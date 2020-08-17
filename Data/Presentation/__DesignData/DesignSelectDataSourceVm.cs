using System;
using System.Collections.Generic;
using System.Text;
using Data.Application.Services;
using Data.Application.ViewModels;

namespace Data.Presentation.__DesignData
{
    class DesignSelectDataSourceVm : SelectDataSourceViewModel
    {
        public DesignSelectDataSourceVm() : base(null)
        {
        }
    }

    class DesignSingleFileSourceVm : SingleFileSourceViewModel
    {
        public DesignSingleFileSourceVm() : base(null)
        {
            SelectedFilePath = "C:\\Users\\Example.csv";
        }
    }
}
