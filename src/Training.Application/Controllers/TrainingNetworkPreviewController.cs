using Common.Domain;
using Common.Framework;
using NNLib.MLP;
using NNLibAdapter;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Windows.Threading;
using Training.Application.Plots;
using Training.Application.ViewModels;
using Training.Domain;


namespace Training.Application.Controllers
{
    public interface ITrainingNetworkPreviewController : IController
    {
        DelegateCommand ToggleAnimationCommand { get; }
        DelegateCommand ClearColorsCommand { get; }
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ITrainingNetworkPreviewController, TrainingNetworkPreviewController>();
        }
    }

    class TrainingNetworkPreviewController : ControllerBase<TrainingNetworkPreviewViewModel>, ITrainingNetworkPreviewController
    {
        private PlotEpochEndConsumer? _epochEndConsumer;
        private Action? _epochEndCallback;
        private readonly ModuleState _moduleState;
        private readonly AppStateHelper _helper;
        private readonly AppState _appState;

        public TrainingNetworkPreviewController(ModuleState moduleState, AppState appState)
        {
            _moduleState = moduleState;
            _appState = appState;
            _helper = new AppStateHelper(appState);

            ToggleAnimationCommand = new DelegateCommand(ToggleAnimation);
            ClearColorsCommand = new DelegateCommand(ClearColors);
            Navigated = NavigatedAction;
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
            Vm!.ModelAdapter.Controller.ForceDraw();
        }


        private void SetupAnimation()
        {
            _epochEndCallback = () => { };
            Vm!.ModelAdapter!.ColorAnimation.SetupTrainer(_moduleState.ActiveSession!.Trainer!, ref _epochEndCallback,
                action => { GlobalDistributingDispatcher.Call(action, _epochEndConsumer!); });
        }

        private void ModuleStateOnActiveSessionChanged(object? sender, (TrainingSession? prev, TrainingSession next) e)
        {
            if (_epochEndConsumer!.IsRunning)
            {
                _epochEndCallback = null;
                _epochEndConsumer?.ForceStop();
            }
        }

        private void NavigatedAction(NavigationContext obj)
        {
            InitializeEpochEndConsumer();

            _moduleState.ActiveSessionChanged += ModuleStateOnActiveSessionChanged;
        }

        private void InitializeEpochEndConsumer()
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
        }

        private void ClearColors()
        {
            Vm!.ModelAdapter!.Controller.Color.ResetColorsToDefault();
        }

        private void ToggleAnimation()
        {
            if (_epochEndConsumer!.IsRunning)
            {
                _epochEndCallback = null;
                _epochEndConsumer?.ForceStop();
            }
            else
            {
                SetupAnimation();
                InitializeEpochEndConsumer();
            }
        }

        public DelegateCommand ToggleAnimationCommand { get; }
        public DelegateCommand ClearColorsCommand { get; }
        public Action<NavigationContext> Navigated { get; }
    }
}