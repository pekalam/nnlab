using Common.Domain;
using Common.Framework;
using Prism.Regions;
using System.Windows;
using Training.Application.Controllers;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingReportRegions
    {
        public const string ReportRegion1 = nameof(ReportRegion1);
        public const string ReportValidationPlotRegion = nameof(ReportValidationPlotRegion);
        public const string ReportTestPlotRegion = nameof(ReportTestPlotRegion);
    }

    public class ReportsViewModel : ViewModelBase<ReportsViewModel>
    {
        private Visibility _testVisibility;
        private Visibility _validationVisibility;
        private string? _testHyperlinkText;
        private string? _validationHyperlinkText;
        private TrainingSessionReport? _selectedReport;

#pragma warning disable 8618
        public ReportsViewModel()
#pragma warning restore 8618
        {
            
        }

        [InjectionConstructor]
        public ReportsViewModel(AppState appState, ModuleState moduleState, IReportsController service)
        {
            AppState = appState;
            ModuleState = moduleState;
            Service = service;
            service.Initialize(this);
        }

        public AppState AppState { get;  }
        public ModuleState ModuleState { get; }
        public IReportsController Service { get; private set; }

        public TrainingSessionReport? SelectedReport
        {
            get => _selectedReport;
            set => SetProperty(ref _selectedReport, value);
        }

        public Visibility TestVisibility
        {
            get => _testVisibility;
            set => SetProperty(ref _testVisibility, value);
        }

        public Visibility ValidationVisibility
        {
            get => _validationVisibility;
            set => SetProperty(ref _validationVisibility, value);
        }

        public string? TestHyperlinkText
        {
            get => _testHyperlinkText;
            set => SetProperty(ref _testHyperlinkText, value);
        }

        public string? ValidationHyperlinkText
        {
            get => _validationHyperlinkText;
            set => SetProperty(ref _validationHyperlinkText, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}
