#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CS8625
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

    class DesignMultiFileSourceVm : MultiFileSourceViewModel
    {
        public DesignMultiFileSourceVm() : base(null)
        {
            
        }
    }

    class DesignCustomDataSetVm : CustomDataSetViewModel
    {
    }

    class DesignDataSetDivisionVm : DataSetDivisionViewModel
    {
        public DesignDataSetDivisionVm() : base(null, null)
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
        public DesignNormalizationViewModel() : base()
        {
        }
    }

    class DesignFileDataSourceViewModel : FileDataSourceViewModel
    {
        public DesignFileDataSourceViewModel() : base(null, null, null)
        {
        }
    }

    class DesignVariablesSelectionVm : VariablesSelectionViewModel
    {

    }

    class DesignStatisticsVm : StatisticsViewModel
    {

    }
}
