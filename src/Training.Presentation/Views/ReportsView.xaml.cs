using CommonServiceLocator;
using Prism.Regions;
using System.Windows.Controls;
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

            if (rm.Regions.ContainsRegionWithName(TrainingReportRegions.ReportValidationPlotRegion))
            {
                rm.Regions.Remove(TrainingReportRegions.ReportValidationPlotRegion);
            }
            if (rm.Regions.ContainsRegionWithName(TrainingReportRegions.ReportTestPlotRegion))
            {
                rm.Regions.Remove(TrainingReportRegions.ReportTestPlotRegion);
            }

            RegionManager.SetRegionName(ReportRegion1, TrainingReportRegions.ReportRegion1);
            RegionManager.SetRegionManager(ReportRegion1, rm);

            RegionManager.SetRegionName(ValidationRegion, TrainingReportRegions.ReportValidationPlotRegion);
            RegionManager.SetRegionManager(ValidationRegion, rm);

            RegionManager.SetRegionName(TestRegion, TrainingReportRegions.ReportTestPlotRegion);
            RegionManager.SetRegionManager(TestRegion, rm);
        }
    }
}
