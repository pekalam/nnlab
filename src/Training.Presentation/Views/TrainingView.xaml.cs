using System.Windows.Controls;
using CommonServiceLocator;
using Prism.Regions;
using Prism.Regions.Behaviors;
using Training.Application;

namespace Training.Presentation.Views
{
    /// <summary>
    /// Interaction logic for TrainingView
    /// </summary>
    public partial class TrainingView : UserControl
    {
        public TrainingView()
        {
            InitializeComponent();
        }

        private void SplitView_Initialized(object sender, System.EventArgs e)
        {
            var rm = ServiceLocator.Current.GetInstance<IRegionManager>();

            if (!rm.Regions.ContainsRegionWithName(TrainingViewRegions.PanelLayoutRegion))
            {
                RegionManager.SetRegionName(PanelLayoutContainer, TrainingViewRegions.PanelLayoutRegion);
                RegionManager.SetRegionManager(PanelLayoutContainer, rm);
                PanelLayoutContainer.SetValue(ClearChildViewsRegionBehavior.ClearChildViewsProperty, true);
            }
 
        }
    }
}
