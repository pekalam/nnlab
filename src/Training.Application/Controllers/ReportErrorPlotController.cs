using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Prism.Ioc;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Services
{
    public interface IReportErrorService : IService
    {
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IReportErrorService, ReportErrorService>().Register<ITransientController<ReportErrorService>, ReportErrorPlotController>();

        }
    }

    internal class ReportErrorService : IReportErrorService
    {
        public ReportErrorService(ITransientController<ReportErrorService> ctrl)
        {
            ctrl.Initialize(this);
        }

        public Action<NavigationContext> Navigated { get; set; }
    }
}

namespace Training.Application.Controllers
{
    internal class ReportErrorPlotController : ITransientController<ReportErrorService>
    {
        private string ReportErrorPlotSettingsRegion;
        private IViewModelAccessor _accessor;
        private IRegionManager _rm;

        public ReportErrorPlotController(IViewModelAccessor accessor)
        {
            _accessor = accessor;
        }

        public void Initialize(ReportErrorService service)
        {
            service.Navigated = Navigated;
        }

        private void Navigated(NavigationContext ctx)
        {
            var parameters = new ReportErrorPlotNavParams.ReportErrorPlotRecNavParams(ctx.Parameters);

            var vm = _accessor.Get<ReportErrorPlotViewModel>();

            ReportErrorPlotSettingsRegion = nameof(ReportErrorPlotSettingsRegion) + parameters.ParentRegion;
            vm.BasicPlotModel.SetSettingsRegion(ReportErrorPlotSettingsRegion);

            vm.Series.Points.Clear();
            vm.Series.Points.AddRange(parameters.Points);
            vm.BasicPlotModel.Model.InvalidatePlot(true);
        }
    }
}
