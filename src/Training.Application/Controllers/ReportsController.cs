using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Common.Domain;
using Common.Framework;
using NNLib.Common;
using OxyPlot;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Services
{
    public interface IReportsService : IService
    {
        DelegateCommand<TrainingSessionReport> SelectionChangedCommand { get; }
        Action<NavigationContext> Navigated { get; }
        DelegateCommand GenerateValidationPlotCommand { get; }
        DelegateCommand GenerateTestPlotCommand { get; }


        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IReportsService, ReportsService>().Register<ITransientController<ReportsService>, ReportsController>();
        }
    }

    internal class ReportsService : IReportsService
    {
        public ReportsService(ITransientController<ReportsService> ctrl)
        {
            ctrl.Initialize(this);
        }

        public DelegateCommand<TrainingSessionReport> SelectionChangedCommand { get; set; } = null!;
        public Action<NavigationContext> Navigated { get; set; } = null!;
        public DelegateCommand GenerateValidationPlotCommand { get; set; } = null!;
        public DelegateCommand GenerateTestPlotCommand { get; set; } = null!;
    }
}

namespace Training.Application.Controllers
{
    class ReportsController : ITransientController<ReportsService>
    {
        private readonly IViewModelAccessor _accessor;
        private readonly IRegionManager _rm;
        private readonly AppState _appState;
        private readonly ModuleState _moduleState;
        private readonly IEventAggregator _ea;

        public ReportsController(IViewModelAccessor accessor, IRegionManager rm, AppState appState, ModuleState moduleState, IEventAggregator ea)
        {
            _accessor = accessor;
            _rm = rm;
            _appState = appState;
            _moduleState = moduleState;
            _ea = ea;
            CloseReportsCommand = new DelegateCommand(CloseReports);
        }

        private DelegateCommand CloseReportsCommand { get; }

        private void ShowErrorPlot(TrainingSessionReport? report)
        {
            if (!_rm.Regions.ContainsRegionWithName(TrainingReportRegions.ReportRegion1)) return;
            //convertion error fix
            if(report == null) return;

            //_rm.Regions[TrainingReportRegions.ReportRegion1].RemoveAll();
            var pts = report.EpochEndEventArgs.Select(v => new DataPoint(v.Epoch, v.Error)).ToList();
            var param = new ReportErrorPlotNavParams(TrainingReportRegions.ReportRegion1, pts);
            _rm.Regions[TrainingReportRegions.ReportRegion1].RequestNavigate("ReportErrorPlotView", param);
        }

        private async void DisplayTVOutputPlot(DataSetType setType)
        {
            var vm = _accessor.Get<ReportsViewModel>()!;

            if (setType == DataSetType.Validation)
            {
                if (vm.SelectedReport!.ValidationError == null)
                {
                    await _moduleState.ActiveSession!.RunValidation(vm.SelectedReport);
                }
            }
            else if (setType == DataSetType.Test)
            {
                if (vm.SelectedReport!.TestError == null)
                {
                    await _moduleState.ActiveSession!.RunTest(vm.SelectedReport);
                }
            }

            var region = setType == DataSetType.Validation
                ? TrainingReportRegions.ReportValidationPlotRegion
                : TrainingReportRegions.ReportTestPlotRegion;

            _rm.Regions[region].RemoveAll();
            

            var param = new OutputPlotNavParams(
                region,
                false,
                setType,
                _appState.ActiveSession!.Network!,
                _appState.ActiveSession.TrainingData!,
                new CancellationTokenSource()
                );
            
            _rm.Regions[region].RequestNavigate("OutputPlotView", param);
        }

        private void CloseReports()
        {
            _ea.GetEvent<DisableModalNavigation>().Publish();
            _rm.NavigateContentRegion("TrainingView");
        }

        public void Initialize(ReportsService service)
        {
            service.SelectionChangedCommand = new DelegateCommand<TrainingSessionReport>(SelectionChanged);
            service.GenerateValidationPlotCommand = new DelegateCommand(() => DisplayTVOutputPlot(DataSetType.Validation));
            service.GenerateTestPlotCommand = new DelegateCommand(() => DisplayTVOutputPlot(DataSetType.Test));
            service.Navigated = Navigated;
        }

        private void InitHyperlinksText()
        {
            var vm = _accessor.Get<ReportsViewModel>()!;
            vm.TestHyperlinkText = vm.SelectedReport!.TestError != null ? "Generate output plot" : "Calculate error and generate output plot";
            vm.ValidationHyperlinkText = vm.SelectedReport.ValidationError != null ? "Generate output plot" : "Calculate error and generate output plot";
        }

        private void ClearTestValidationRegions()
        {
            _rm.Regions[TrainingReportRegions.ReportValidationPlotRegion].RemoveAll();
            _rm.Regions[TrainingReportRegions.ReportTestPlotRegion].RemoveAll();
        }

        private void Navigated(NavigationContext ctx)
        {
            _ea.GetEvent<EnableModalNavigation>().Publish(CloseReportsCommand);
            ShowErrorPlot(_appState.ActiveSession!.TrainingReports[0]);
            InitHyperlinksText();
        }

        private void SelectionChanged(TrainingSessionReport item)
        {
            if (item == null) return;
            ShowErrorPlot(item);
            ClearTestValidationRegions();
            InitHyperlinksText();
        }
    }
}
