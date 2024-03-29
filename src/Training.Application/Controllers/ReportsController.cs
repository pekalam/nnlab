﻿using Common.Domain;
using Common.Framework;
using NNLib.Data;
using OxyPlot;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Training.Application.ViewModels;
using IController = Common.Framework.IController;

namespace Training.Application.Controllers
{
    public interface IReportsController : IController
    {
        DelegateCommand<TrainingSessionReport> SelectionChangedCommand { get; }
        Action<NavigationContext> Navigated { get; }
        DelegateCommand GenerateValidationPlotCommand { get; }
        DelegateCommand GenerateTestPlotCommand { get; }


        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IReportsController, ReportsController>();
        }
    }

    class ReportsController : ControllerBase<ReportsViewModel>,IReportsController
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

            var pts = report.EpochEndEventArgs.Select(v => new DataPoint(v.Epoch, v.Error)).ToList();
            List<DataPoint>? valPoints = null;
            if (report.EpochEndEventArgs.Length > 0 && report.EpochEndEventArgs[0].ValidationError.HasValue)
            {
                valPoints = report.EpochEndEventArgs.Select(v => new DataPoint(v.Epoch, v.ValidationError!.Value)).ToList();
            }
            var param = new ErrorPlotNavParams(TrainingReportRegions.ReportRegion1, false, pts, valPoints);
            _rm.Regions[TrainingReportRegions.ReportRegion1].RequestNavigate("ErrorPlotView", param);
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
                Vm!.SelectedReport!.Network,
                _appState.ActiveSession!.TrainingData!,
                new CancellationTokenSource()
                );
            
            _rm.Regions[region].RequestNavigate("OutputPlotView", param);
        }

        private void CloseReports()
        {
            _rm.Regions[TrainingReportRegions.ReportRegion1].RemoveAll();
            _rm.Regions[TrainingReportRegions.ReportTestPlotRegion].RemoveAll();
            _rm.Regions[TrainingReportRegions.ReportValidationPlotRegion].RemoveAll();
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
