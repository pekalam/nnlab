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
                case Panels.ParametersEditPanel: return "ParametersEditView";
                default: throw new Exception("Invalid panel type");
            }
        }
    }

    public abstract class LayoutViewModelBase : ViewModelBase<LayoutViewModelBase>
    {
        protected IRegionManager Rm;
        protected NavigationParameters InitialNavParams = null!;

        protected LayoutViewModelBase(IRegionManager rm)
        {
            Rm = rm;
            IsActiveChanged += OnIsActiveChanged;
        }

        private void OnIsActiveChanged(object? sender, EventArgs e)
        {
            if (!IsActive)
            {
                OnInactive();
                IsActiveChanged -= OnIsActiveChanged;
            }
        }

        protected void ClearAndNavgate(string regionName, string view, NavigationParameters? navParams = null)
        {
            Rm.Regions[regionName].RemoveAll();
            Rm.Regions[regionName].RequestNavigate(view, navParams);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            InitialNavParams = navigationContext.Parameters;
            var selected = navigationContext.Parameters["selectedPanels"] as List<PanelSelectModel>;
            OnPanelsSelected(selected!.Select(PanelToViewHelper.GetView).ToArray(), selected!, navigationContext.Parameters);
        }

        protected abstract void OnPanelsSelected(string[] views, List<PanelSelectModel> selected, NavigationParameters navParams);
        protected abstract void OnInactive();
    }

    public class PanelLayoutViewModel : ViewModelBase<PanelLayoutViewModel>
    {
        private readonly IRegionManager _rm;

        public PanelLayoutViewModel(IRegionManager rm)
        {
            _rm = rm;
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            var selected = (navigationContext.Parameters["selectedPanels"] as List<PanelSelectModel>)!;
            _rm.Regions[PanelLayoutRegions.PanelLayoutMain].RemoveAll();
            switch (selected.Count)
            {
                case 1: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "SingleLayoutView",navigationContext.Parameters); break;
                case 2: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "Horizontal2LayoutView",navigationContext.Parameters); break;
                case 3: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "Part3LayoutView",navigationContext.Parameters); break;
                case 4: _rm.RequestNavigate(PanelLayoutRegions.PanelLayoutMain, "Part4LayoutView",navigationContext.Parameters); break;
            }
        }


    }

    public class PanelLayoutNavigationParams : NavigationParameters
    {
        public PanelLayoutNavigationParams(List<PanelSelectModel> selectedPanels, List<(string name, string value)>? additionalFields = null)
        {
            Add("selectedPanels", selectedPanels);
            if (additionalFields != null)
            {
                for (int i = 0; i < additionalFields.Count; i++)
                {
                    Add(additionalFields[i].name, additionalFields[i].value);
                }
            }
        }
    }
}
