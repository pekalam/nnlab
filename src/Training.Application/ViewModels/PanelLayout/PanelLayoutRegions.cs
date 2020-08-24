using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Prism.Regions;
using System.Linq;
using System.Windows.Media.Animation;

namespace Training.Application.ViewModels.PanelLayout
{
    public class PanelLayoutRegions
    {
        public const string PanelLayoutMain = nameof(PanelLayoutMain);
    }

    public class SingleLayoutRegions
    {
        public const string SingleLayoutMainRegion = nameof(SingleLayoutMainRegion);
    }

    public static class PanelToViewHelper
    {
        public static string GetView(PanelSelectModel model)
        {
            switch (model.PanelType)
            {
                case Panels.MatrixPreview: return "MatrixTrainingPreviewView";
                case Panels.Accuracy: return "OutputPlotView";
                case Panels.NetworkError: return "ErrorPlotView";
                case Panels.NetworkVisualization: return "TrainingNetworkPreviewView";
                case Panels.LiveEditPanel: return "ParametersEditView";
                default: throw new Exception("Invalid panel type");
            }
        }
    }

    public abstract class LayoutViewModelBase : ViewModelBase<LayoutViewModelBase>
    {
        protected IRegionManager Rm;

        protected LayoutViewModelBase(IRegionManager rm)
        {
            Rm = rm;
        }

        protected void ClearAndNavgate(string regionName, string view, NavigationParameters navParams = null)
        {
            Rm.Regions[regionName].RemoveAll();
            Rm.Regions[regionName].RequestNavigate(view, navParams);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var selected = navigationContext.Parameters["selectedPanels"] as List<PanelSelectModel>;
            OnPanelsSelected(selected.Select(PanelToViewHelper.GetView).ToArray(), selected);
        }

        protected abstract void OnPanelsSelected(string[] views, List<PanelSelectModel> selected);
    }

    public class PanelLayoutViewModel : ViewModelBase<PanelLayoutViewModel>
    {
        private readonly IRegionManager _rm;
        private bool _initialized;

        public PanelLayoutViewModel(IRegionManager rm)
        {
            _rm = rm;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (_initialized) return;
            //_initialized = true;
            var selected = navigationContext.Parameters["selectedPanels"] as List<PanelSelectModel>;
            var navParams = new NavigationParameters();
            navParams.Add("selectedPanels", selected);
            switch (selected.Count)
            {
                case 1: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "SingleLayoutView",navParams); break;
                case 2: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "Horizontal2LayoutView",navParams); break;
                case 3: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "Part3LayoutView",navParams); break;
                case 4: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "Part4LayoutView",navParams); break;
            }
        }


    }

    public class PanelLayoutNavigationParams : NavigationParameters
    {
        public PanelLayoutNavigationParams(List<PanelSelectModel> selectedPanels)
        {
            Add("selectedPanels", selectedPanels);
        }
    }
}
