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

        public Action<NavigationContext> Navigated { get; set; } = null!;
    }
}

namespace Training.Application.Controllers
{
    internal class ReportErrorPlotController : ITransientController<ReportErrorService>
    {
        private string? _reportErrorPlotSettingsRegion;
        private readonly IViewModelAccessor _accessor;

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

            var vm = _accessor.Get<ReportErrorPlotViewModel>()!;

            _reportErrorPlotSettingsRegion = nameof(_reportErrorPlotSettingsRegion) + parameters.ParentRegion;
            vm.BasicPlotModel.SetSettingsRegion?.Invoke(_reportErrorPlotSettingsRegion);

            vm.Series.Points.Clear();
            vm.Series.Points.AddRange(parameters.Points);
            vm.BasicPlotModel.Model.InvalidatePlot(true);
        }
    }
}
