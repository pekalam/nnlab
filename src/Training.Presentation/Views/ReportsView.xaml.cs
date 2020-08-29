using System.Windows.Controls;
using Common.Domain;
using CommonServiceLocator;
using Prism.Regions;
using Training.Application.ViewModels;

namespace Training.Presentation.Views
{
    /// <summary>
    /// Interaction logic for ReportsView
    /// </summary>
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
            var rm = ServiceLocator.Current.GetInstance<IRegionManager>();

            if (rm.Regions.ContainsRegionWithName(TrainingReportRegions.ReportRegion1))
            {
                rm.Regions.Remove(TrainingReportRegions.ReportRegion1);

            }

            if (!rm.Regions.ContainsRegionWithName(TrainingReportRegions.ReportRegion1))
            {
                
                RegionManager.SetRegionName(ReportRegion1, TrainingReportRegions.ReportRegion1);
                RegionManager.SetRegionManager(ReportRegion1, rm);
            }
            if (!rm.Regions.ContainsRegionWithName(TrainingReportRegions.ReportValidationPlotRegion))
            {
                RegionManager.SetRegionName(ValidationRegion, TrainingReportRegions.ReportValidationPlotRegion);
                RegionManager.SetRegionManager(ValidationRegion, rm);
            }
            if (!rm.Regions.ContainsRegionWithName(TrainingReportRegions.ReportTestPlotRegion))
            {
                RegionManager.SetRegionName(TestRegion, TrainingReportRegions.ReportTestPlotRegion);
                RegionManager.SetRegionManager(TestRegion, rm);
            }

        }
    }
}
