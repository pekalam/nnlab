using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Common.Domain;
using Common.Framework;
using NNLib.Common;
using NNLib.Data;
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
    public interface IReportsService : ITransientController
    {
        DelegateCommand<TrainingSessionReport> SelectionChangedCommand { get; }
        Action<NavigationContext> Navigated { get; }
        DelegateCommand GenerateValidationPlotCommand { get; }
        DelegateCommand GenerateTestPlotCommand { get; }


        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IReportsService, ReportsController>();
        }
    }
}

namespace Training.Application.Controllers
{
    class ReportsController : ControllerBase<ReportsViewModel>,IReportsService
    {
        private readonly IRegionManager _rm;
        private readonly AppState _appState;
        private readonly ModuleState _moduleState;
        private readonly IEventAggregator _ea;

        public ReportsController(IRegionManager rm, AppState appState, ModuleState moduleState, IEventAggregator ea)
        {
            _rm = rm;
            _appState = appState;
            _moduleState = moduleState;
            _ea = ea;
            CloseReportsCommand = new DelegateCommand(CloseReports);

            SelectionChangedCommand = new DelegateCommand<TrainingSessionReport>(SelectionChanged);
            GenerateValidationPlotCommand = new DelegateCommand(() => DisplayTVOutputPlot(DataSetType.Validation));
            GenerateTestPlotCommand = new DelegateCommand(() => DisplayTVOutputPlot(DataSetType.Test));
            Navigated = NavigatedAction;
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
            if (setType == DataSetType.Validation)
            {
                if (Vm!.SelectedReport!.ValidationError == null)
                {
                    await _moduleState.ActiveSession!.RunValidation(Vm!.SelectedReport);
                }
            }
            else if (setType == DataSetType.Test)
            {
                if (Vm!.SelectedReport!.TestError == null)
                {
                    await _moduleState.ActiveSession!.RunTest(Vm!.SelectedReport);
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

        private void InitHyperlinksText()
        {
            Vm!.TestHyperlinkText = Vm!.SelectedReport!.TestError != null ? "Generate output plot" : "Calculate error and generate output plot";
            Vm!.ValidationHyperlinkText = Vm!.SelectedReport.ValidationError != null ? "Generate output plot" : "Calculate error and generate output plot";
        }

        private void ClearTestValidationRegions()
        {
            _rm.Regions[TrainingReportRegions.ReportValidationPlotRegion].RemoveAll();
            _rm.Regions[TrainingReportRegions.ReportTestPlotRegion].RemoveAll();
        }

        private void NavigatedAction(NavigationContext ctx)
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

        public Action<NavigationContext> Navigated { get; }
        public DelegateCommand GenerateValidationPlotCommand { get; }
        public DelegateCommand GenerateTestPlotCommand { get; }
        public DelegateCommand<TrainingSessionReport> SelectionChangedCommand { get; }
    }
}
