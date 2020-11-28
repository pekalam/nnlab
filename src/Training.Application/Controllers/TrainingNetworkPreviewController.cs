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
                    Vm!.ModelAdapter = new NNLibModelAdapter(network);
                }
                else
                {
                    Vm!.ModelAdapter.SetNeuralNetwork(network);
                }

                Vm!.ModelAdapter.NeuralNetworkModel.BackgroundColor = "#cce6ff";
                if (_appState.ActiveSession!.TrainingData != null)
                {
                    Vm!.ModelAdapter.AttachInputLabels(_appState.ActiveSession!.TrainingData.Variables.InputVariableNames);
                    Vm!.ModelAdapter.AttachOutputLabels(_appState.ActiveSession.TrainingData.Variables.TargetVariableNames);
                }

            });

            _helper.OnTrainingDataChanged(data =>
            {
                Vm!.ModelAdapter.AttachInputLabels(_appState.ActiveSession!.TrainingData.Variables.InputVariableNames);
                Vm!.ModelAdapter.AttachOutputLabels(_appState.ActiveSession.TrainingData.Variables.TargetVariableNames);
            });

            Vm!.IsActiveChanged += OnIsActiveChanged;
        }

        private void OnIsActiveChanged(object? sender, EventArgs e)
        {
            if (!Vm!.IsActive)
            {
                _epochEndCallback = null;
                _epochEndConsumer?.Remove();
                Vm!.IsActiveChanged -= OnIsActiveChanged;
            }
        }

        private void ActiveSessionOnNetworkStructureChanged(MLPNetwork obj)
        {
            Vm!.ModelAdapter!.Controller.ForceDraw();
        }


        private void SetupAnimation()
        {
            _epochEndCallback = () => { };
            Vm!.ModelAdapter!.ColorAnimation.SetupTrainer(_moduleState.ActiveSession!.Trainer!, ref _epochEndCallback,
                action => { GlobalDistributingDispatcher.Call(action, _epochEndConsumer!); });
        }

        private void NavigatedAction(NavigationContext obj)
        {
            InitializeEpochEndConsumer();
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
                _epochEndConsumer?.Remove();
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