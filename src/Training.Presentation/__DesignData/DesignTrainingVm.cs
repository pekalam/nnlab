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
            a.ActiveSession!.TrainingReports.Add(TrainingSessionReport.CreateTimeoutSessionReport(50, .2, DateTime.Now, new List<EpochEndArgs>()));
            a.ActiveSession.TrainingReports.Add(TrainingSessionReport.CreatePausedSessionReport(100, .06, DateTime.Now, new List<EpochEndArgs>()));
            a.ActiveSession.TrainingReports.Add(TrainingSessionReport.CreateStoppedSessionReport(2100, .12, DateTime.Now, new List<EpochEndArgs>()));
        }
    }

    class DesignParametersEditVm : ParametersEditViewModel
    {

    }
}
