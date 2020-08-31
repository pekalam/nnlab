using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using CommonServiceLocator;
using Prism.Ioc;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.Plots;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Services
{
    interface IMatrixTrainingPreviewService : IService
    {
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IMatrixTrainingPreviewService, MatrixTrainingPreviewService>()
                .Register<ITransientController<MatrixTrainingPreviewService>, MatrixTrainingPreviewController>();
        }
    }

    class MatrixTrainingPreviewService : IMatrixTrainingPreviewService
    {
        public Action<NavigationContext> Navigated { get; set; } = null!;

        public MatrixTrainingPreviewService(ITransientController<MatrixTrainingPreviewService> ctrl)
        {
            ctrl.Initialize(this);
        }
    }
}

namespace Training.Application.Controllers
{
    class MatrixTrainingPreviewController : ControllerBase<MatrixTrainingPreviewViewModel>,ITransientController<MatrixTrainingPreviewService>
    {
        private PlotEpochEndConsumer? _epochEndConsumer;
        private readonly ModuleState _moduleState;

        public MatrixTrainingPreviewController(ModuleState moduleState, IViewModelAccessor accessor) : base(accessor)
        {
            _moduleState = moduleState;
        }

        protected override void VmCreated()
        {
            Vm!.IsActiveChanged += (_, __) =>
            {
                if (!Vm!.IsActive) _epochEndConsumer!.ForceStop();
            };
        }

        public void Initialize(MatrixTrainingPreviewService service)
        {
            service.Navigated = Navigated;
        }

        private void AssignSession(TrainingSession session)
        {
            Vm!.MatVm!.Controller.AssignNetwork(session.Network!);
        }

        private void Navigated(NavigationContext parameters)
        {


            if (_moduleState.ActiveSession != null)
            {
                AssignSession(_moduleState.ActiveSession);
            }

            _moduleState.ActiveSessionChanged += (_, args) => AssignSession(args.next);


            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState,(list, session) =>
            {
                Vm!.MatVm!.Controller.Update();
                GlobalDistributingDispatcher.Call(() =>
                {
                    Vm!.MatVm!.Controller.ApplyUpdate();
                }, _epochEndConsumer!);
            });

            _epochEndConsumer.Initialize();
        }
    }
}
