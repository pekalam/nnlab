using Common.Framework;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Drawing.Printing;
using NNLib;
using Training.Application.Plots;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Controllers
{
    interface IMatrixTrainingPreviewController : IController
    {
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IMatrixTrainingPreviewController, MatrixTrainingPreviewController>();
        }
    }

    class MatrixTrainingPreviewController : ControllerBase<MatrixTrainingPreviewViewModel>,IMatrixTrainingPreviewController
    {
        private PlotEpochEndConsumer? _epochEndConsumer;
        private readonly ModuleState _moduleState;
        private readonly ModuleStateHelper _helper;

        public MatrixTrainingPreviewController(ModuleState moduleState, ModuleStateHelper helper)
        {
            _moduleState = moduleState;
            _helper = helper;
            Navigated = NavigatedAction;
        }

        protected override void VmCreated()
        {
            Vm!.IsActiveChanged += OnIsActiveChanged;
        }

        private void OnIsActiveChanged(object? sender, EventArgs e)
        {
            if (!Vm!.IsActive)
            {
                _epochEndConsumer?.Remove();
                Vm!.IsActiveChanged -= OnIsActiveChanged;
            }
        }

        private void AssignSession(TrainingSession session)
        {
            if (session.Network != null)
            {
                Vm!.MatVm!.Controller.AssignNetwork(session.Network);

                session.Network.StructureChanged -= NetworkOnStructureChanged;
                session.Network.StructureChanged += NetworkOnStructureChanged;
            }
        }

        private void NetworkOnStructureChanged(INetwork obj)
        {
            Vm!.MatVm!.Controller.AssignNetwork(_moduleState.ActiveSession!.Network!);
        }

        private void NavigatedAction(NavigationContext parameters)
        {
            if (_moduleState.ActiveSession != null)
            {
                AssignSession(_moduleState.ActiveSession);
            }

            _helper.OnTrainerChanged(trainer =>
            {
                AssignSession(_moduleState.ActiveSession!);
            });

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

        public Action<NavigationContext> Navigated { get; }
    }
}
