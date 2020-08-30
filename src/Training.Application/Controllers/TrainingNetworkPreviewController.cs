using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Common.Framework;
using Prism.Commands;
using Prism.Ioc;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Services
{
    public interface ITrainingNetworkPreviewService : IService
    {
        DelegateCommand ToggleAnimationCommand { get; }
        DelegateCommand ClearColorsCommand { get; }

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

        public DelegateCommand ToggleAnimationCommand { get; set; }
        public DelegateCommand ClearColorsCommand { get; set; }
    }
}

namespace Training.Application.Controllers
{
    class TrainingNetworkPreviewController : ControllerBase<TrainingNetworkPreviewViewModel>,ITransientController<TrainingNetworkPreviewService>
    {
        private IViewModelAccessor _accessor;
        private readonly PlotEpochEndConsumer _epochEndConsumer;
        private Action _epochEndCallback = () => { };
        private CancellationTokenSource _cts;
        private readonly ModuleState _moduleState;

        public TrainingNetworkPreviewController(IViewModelAccessor accessor, ModuleState moduleState) : base(accessor)
        {
            _accessor = accessor;
            _moduleState = moduleState;

            _epochEndConsumer = new PlotEpochEndConsumer(moduleState, (_, __) => _epochEndCallback(),
                session =>
                {
                    SetupAnimation();
                },
                session =>
                {
                    Vm.ModelAdapter.ColorAnimation.StopAnimation(false);
                    _cts.Cancel();
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Vm.ModelAdapter.Controller.Color.ApplyColors(), DispatcherPriority.Background);
                },
                session =>
                {
                    
                    _cts.Cancel();
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(() => Vm.ModelAdapter.Controller.Color.ApplyColors(), DispatcherPriority.Background);
                });
            _epochEndConsumer.Initialize();

            moduleState.ActiveSessionChanged += ModuleStateOnActiveSessionChanged;

            accessor.OnCreated<TrainingNetworkPreviewViewModel>(() =>
            {
                
                Vm.IsActiveChanged += (sender, args) =>
                {
                    if(!Vm.IsActive) _epochEndConsumer.ForceStop();
                };
            });
        }


        private void SetupAnimation()
        {
            _cts = new CancellationTokenSource();
            Vm.ModelAdapter.ColorAnimation.SetupTrainer(_moduleState.ActiveSession.Trainer, ref _epochEndCallback, action =>
            {
                GlobalDistributingDispatcher.Call(() =>
                {
                    if (System.Windows.Application.Current == null) return;

                    System.Windows.Application.Current.Dispatcher.InvokeAsync(action, DispatcherPriority.Background, _cts.Token);
                }, _epochEndConsumer);
            });
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            Vm.ModelAdapter.ColorAnimation.StopAnimation(true);
        }

        public void Initialize(TrainingNetworkPreviewService service)
        {
            service.ToggleAnimationCommand = new DelegateCommand(ToggleAnimation);
            service.ClearColorsCommand = new DelegateCommand(ClearColors);
        }

        private void ClearColors()
        {
            Vm.StopAnimation(true);
        }

        private void ToggleAnimation()
        {
            if (Vm.ModelAdapter.ColorAnimation.IsAnimating)
            {
                Vm.ModelAdapter.ColorAnimation.StopAnimation(false);
            }
            else
            {
                SetupAnimation();
            }
        }
    }
}