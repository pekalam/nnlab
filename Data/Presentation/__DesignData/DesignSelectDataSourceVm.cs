using System;
using System.Collections.Generic;
using System.Text;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Application.ViewModels.CustomDataSet;
using Data.Application.ViewModels.DataSetDivision;
using Data.Application.ViewModels.DataSource.FileDataSource;
using Data.Application.ViewModels.DataSource.Normalization;
using Data.Application.ViewModels.DataSource.Preview;

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

    class DesignMultiFileSourceVm : MultiFileSourceViewModel
    {
        public DesignMultiFileSourceVm() : base(null)
        {
            
        }
    }

    class DesignCustomDataSetVm : CustomDataSetViewModel
    {
        public DesignCustomDataSetVm(ICustomDataSetService customDataSetService) : base(null)
        {
        }
    }

    class DesignDataSetDivisionVm : DataSetDivisionViewModel
    {
        public DesignDataSetDivisionVm() : base(null, null)
        {
        }
    }

    class DesignDataSetDivisionActionLeftVm : ActionMenuLeftViewModel
    {
        public DesignDataSetDivisionActionLeftVm() : base(null)
        {
        }
    }

    class DesignFileDataSourceVm : FileDataSourceViewModel
    {
        public DesignFileDataSourceVm() : base(null,null,null)
        {
        }
    }

    class DesignNormalizationViewModel : NormalizationViewModel
    {
        public DesignNormalizationViewModel() : base(null)
        {
        }
    }
}
