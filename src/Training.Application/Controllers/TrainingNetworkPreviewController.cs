using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using Common.Domain;
using Common.Framework;
using NNLib;
using NNLib.MLP;
using NNLibAdapter;
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
    class TrainingNetworkPreviewController : ControllerBase<TrainingNetworkPreviewViewModel>,
        ITransientController<TrainingNetworkPreviewService>
    {
        private PlotEpochEndConsumer? _epochEndConsumer;
        private Action? _epochEndCallback;
        private readonly ModuleState _moduleState;
        private readonly AppStateHelper _helper;
        private readonly AppState _appState;

        public TrainingNetworkPreviewController(IViewModelAccessor accessor, ModuleState moduleState, AppState appState)
            : base(accessor)
        {
            _moduleState = moduleState;
            _appState = appState;
            _helper = new AppStateHelper(appState);
        }

        protected override void VmCreated()
        {
            _helper.OnNetworkChanged(network =>
            {
                _appState.ActiveSession!.NetworkStructureChanged -= ActiveSessionOnNetworkStructureChanged;
                _appState.ActiveSession!.NetworkStructureChanged += ActiveSessionOnNetworkStructureChanged;

                if (Vm!.ModelAdapter == null)
                {
                    Vm!.ModelAdapter = new NNLibModelAdapter();
                }

                Vm!.ModelAdapter.SetNeuralNetwork(network);
                Vm!.ModelAdapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";
                Vm!.ModelAdapter.SetInputLabels(_appState.ActiveSession!.TrainingData!.Variables.InputVariableNames);
                Vm!.ModelAdapter.SetOutputLabels(_appState.ActiveSession.TrainingData.Variables.TargetVariableNames);
            });
            Vm!.IsActiveChanged += OnIsActiveChanged;
        }

        private void OnIsActiveChanged(object? sender, EventArgs e)
        {
            if (!Vm!.IsActive)
            {
                _epochEndCallback = null;
                _epochEndConsumer?.ForceStop();
                Vm!.IsActiveChanged -= OnIsActiveChanged;
            }
        }

        private void ActiveSessionOnNetworkStructureChanged(MLPNetwork obj)
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;
            Vm!.ModelAdapter!.SetNeuralNetwork(obj);
            Vm!.ModelAdapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";

            if (obj.Layers[0].InputsCount != trainingData.Variables.InputVariableNames.Length ||
                Vm!.ModelAdapter.LayerModelAdapters[^1].LayerModel.NeuronModels.Count != trainingData.Variables.TargetVariableNames.Length) return;

            Vm!.ModelAdapter.SetInputLabels(trainingData.Variables.InputVariableNames);
            Vm!.ModelAdapter.SetOutputLabels(trainingData.Variables.TargetVariableNames);
        }


        private void SetupAnimation()
        {
            _epochEndCallback = () => { };
            Vm!.ModelAdapter!.ColorAnimation.SetupTrainer(_moduleState.ActiveSession!.Trainer!, ref _epochEndCallback,
                action => { GlobalDistributingDispatcher.Call(action, _epochEndConsumer!); });
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            if (Vm!.ModelAdapter!.ColorAnimation.IsAnimating)
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
            _epochEndConsumer = new PlotEpochEndConsumer(_moduleState, (_, __) => _epochEndCallback!(),
                session => { SetupAnimation(); },
                session =>
                {
                    Vm!.ModelAdapter!.ColorAnimation.StopAnimation(false);
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(
                        Vm!.ModelAdapter.Controller.Color.ApplyColors, DispatcherPriority.Background);
                },
                session =>
                {
                    System.Windows.Application.Current.Dispatcher.InvokeAsync(
                        Vm!.ModelAdapter!.Controller.Color.ApplyColors, DispatcherPriority.Background);
                });
            _epochEndConsumer.Initialize();

            _moduleState.ActiveSessionChanged += ModuleStateOnActiveSessionChanged;
        }

        private void ClearColors()
        {
            Vm!.ModelAdapter!.Controller.Color.ResetColorsToDefault();
        }

        private void ToggleAnimation()
        {
            if (Vm!.ModelAdapter!.ColorAnimation.IsAnimating)
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