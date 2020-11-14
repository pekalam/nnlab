using Common.Domain;
using System;
using System.Collections.Generic;
using Training.Application;
using Training.Application.ViewModels;

namespace Training.Presentation.__DesignData
{
    class DesignTrainingVm : TrainingViewModel
    {

    }

    class DesignPanelSelectVm : PanelSelectViewModel
    {

    }

    class DesignTrainingInfoVm : TrainingInfoViewModel { }

    class DesignTrainingParametersVm : TrainingParametersViewModel { }

    class DesignReportsVm : ReportsViewModel
    {
        private static AppState a = new AppState();

        public DesignReportsVm() : base(a, new ModuleState(a), null!)
        {
            a.CreateSession();
        }
    }

    class DesignParametersEditVm : ParametersEditViewModel
    {

    }
}
