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
        public Action<NavigationContext> Navigated { get; set; }

        public MatrixTrainingPreviewService(ITransientController<MatrixTrainingPreviewService> ctrl)
        {
            ctrl.Initialize(this);
        }
    }
}

namespace Training.Application.Controllers
{
    class MatrixTrainingPreviewController : ITransientController<MatrixTrainingPreviewService>
    {
        private PlotEpochEndConsumer? _epochEndConsumer;
        private readonly ModuleState _moduleState;
        private IViewModelAccessor _accessor;

        public MatrixTrainingPreviewController(ModuleState moduleState, IViewModelAccessor accessor)
        {
            _moduleState = moduleState;
            _accessor = accessor;
        }


        public void Initialize(MatrixTrainingPreviewService service)
        {
            service.Navigated = Navigated;
        }

        private void AssignSession(TrainingSession session)
        {
            var vm = _accessor.Get<MatrixTrainingPreviewViewModel>();
            vm.MatVm.Controller.AssignNetwork(session.Network);
        }

        private void Navigated(NavigationContext parameters)
        {
            var vm = _accessor.Get<MatrixTrainingPreviewViewModel>();


            if (_moduleState.ActiveSession != null)
            {
                AssignSession(_moduleState.ActiveSession);
            }

            _moduleState.ActiveSessionChanged += (_, args) => AssignSession(args.next);


            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState,(list, session) =>
            {
                vm.MatVm.Controller.Update();
                GlobalDistributingDispatcher.Call(() =>
                {
                    vm.MatVm.Controller.ApplyUpdate();
                }, _epochEndConsumer);
            });

            _epochEndConsumer.Initialize();
        }
    }
}
