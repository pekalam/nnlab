using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Common.Framework;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.Plots;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Application.ViewModels.PanelLayout;
using Training.Domain;

namespace Training.Application.Services
{
    public interface ITrainingNetworkPreviewService : IService
    {
        DelegateCommand ToggleAnimationCommand { get; }
        DelegateCommand ClearColorsCommand { get; }
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ITrainingNetworkPreviewService, TrainingNetworkPreviewService>()
                .Register<ITransientController<TrainingNetworkPreviewService>, TrainingNetworkPreviewController>();
        }
    }

    internal class TrainingNetworkPreviewService : ITrainingNetworkPreviewService
    {
        public TrainingNetworkPreviewService(ITransientController<TrainingNetworkPreviewService> ctrl)
        {
            ctrl.Initialize(this);
        }

        public DelegateCommand ToggleAnimationCommand { get; set; } = null!;
        public DelegateCommand ClearColorsCommand { get; set; } = null!;
        public Action<NavigationContext> Navigated { get; set; } = null!;
    }
}

namespace Training.Application.Controllers
{
    class TrainingNetworkPreviewController : ControllerBase<TrainingNetworkPreviewViewModel>,ITransientController<TrainingNetworkPreviewService>
    {
        private PlotEpochEndConsumer? _epochEndConsumer;
        private Action _epochEndCallback = () => { };
        private readonly ModuleState _moduleState;

        public TrainingNetworkPreviewController(IViewModelAccessor accessor, ModuleState moduleState) : base(accessor)
        {
            _moduleState = moduleState;
        }

        protected override void VmCreated()
        {
            viewModelAccessor.Get<PanelLayoutViewModel>()!.IsActiveChanged += (sender, args) =>
            {
                if (!(sender as PanelLayoutViewModel)!.IsActive)
                {
                    _epochEndConsumer?.ForceStop();
                }
            };

            Vm!.IsActiveChanged += (sender, args) =>
            {
                if (!Vm!.IsActive) _epochEndConsumer!.ForceStop();
            };
        }


        private void SetupAnimation()
        {
            Vm!.ModelAdapter.ColorAnimation.SetupTrainer(_moduleState.ActiveSession!.Trainer!, ref _epochEndCallback, action =>
            {
                GlobalDistributingDispatcher.Call(action, _epochEndConsumer!);
            });
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            if (Vm!.ModelAdapter.ColorAnimation.IsAnimating)
            {
                Vm!.ModelAdapter.ColorAnimation.StopAnimation(true);
            }
        }

        public void Initialize(TrainingNetworkPreviewService service)
        {
            service.ToggleAnimationCommand = new DelegateCommand(ToggleAnimation);
            service.ClearColorsCommand = new DelegateCommand(ClearColors);
            service.Navigated = Navigated;
        }

        private void Navigated(NavigationContext obj)
        {
            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState, (_, __) => _epochEndCallback(),
                session =>
                {
                    SetupAnimation();
                },
                session =>
                {
                    Vm!.ModelAdapter.ColorAnimation.StopAnimation(false);
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Vm!.ModelAdapter.Controller.Color.ApplyColors(), DispatcherPriority.Background);
                },
                session =>
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Vm!.ModelAdapter.Controller.Color.ApplyColors(), DispatcherPriority.Background);
                });
            _epochEndConsumer.Initialize();

            _moduleState.ActiveSessionChanged += ModuleStateOnActiveSessionChanged;
        }

        private void ClearColors()
        {
            Vm!.ModelAdapter.Controller.Color.ResetColorsToDefault();
        }

        private void ToggleAnimation()
        {
            if (Vm!.ModelAdapter.ColorAnimation.IsAnimating)
            {
                Vm!.ModelAdapter.ColorAnimation.StopAnimation(false);
            }
            else
            {
                SetupAnimation();
            }
        }
    }
}