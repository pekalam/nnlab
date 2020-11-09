using Common.Framework;
using Prism.Ioc;
using Prism.Regions;
using System;
using Training.Application.ViewModels;

namespace Training.Application.Controllers
{
    public interface IReportErrorController : IController
    {
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IReportErrorController, ReportErrorPlotController>();

        }
    }

    internal class ReportErrorPlotController : ControllerBase<ReportErrorPlotViewModel>,IReportErrorController
    {
        private string? _reportErrorPlotSettingsRegion;

        public ReportErrorPlotController()
        {
            Navigated = NavigatedAction;
        }

        public void Initialize()
        {
        }

        private void NavigatedAction(NavigationContext ctx)
        {
            var parameters = new ReportErrorPlotNavParams.ReportErrorPlotRecNavParams(ctx.Parameters);

            _reportErrorPlotSettingsRegion = nameof(_reportErrorPlotSettingsRegion) + parameters.ParentRegion;
            Vm!.BasicPlotModel.SetSettingsRegion?.Invoke(_reportErrorPlotSettingsRegion);
            Vm!.Series.Points.Clear();
            Vm!.Series.Points.AddRange(parameters.Points);
            Vm!.BasicPlotModel.Model.InvalidatePlot(true);
        }

        public Action<NavigationContext> Navigated { get; }
    }
}
