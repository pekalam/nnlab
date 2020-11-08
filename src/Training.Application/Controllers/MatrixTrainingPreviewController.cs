using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using CommonServiceLocator;
using Prism.Ioc;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.Plots;
using Training.Application.ViewModels;
using Training.Application.ViewModels.PanelLayout;
using Training.Domain;

namespace Training.Application.Controllers
{
    interface IMatrixTrainingPreviewController : ITransientController
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

        public MatrixTrainingPreviewController(ModuleState moduleState)
        {
            _moduleState = moduleState;
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
                _epochEndConsumer?.ForceStop();
                Vm!.IsActiveChanged -= OnIsActiveChanged;
            }
        }

        private void AssignSession(TrainingSession session)
        {
            Vm!.MatVm!.Controller.AssignNetwork(session.Network!);
        }

        private void NavigatedAction(NavigationContext parameters)
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

        public Action<NavigationContext> Navigated { get; }
    }
}
