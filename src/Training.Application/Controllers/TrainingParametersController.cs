using Common.Domain;
using Common.Framework;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Training.Application.ViewModels;
using Training.Application.Views;

namespace Training.Application.Controllers
{
    public interface ITrainingParametersController : IController
    {
        DelegateCommand OkCommand { get; }
        DelegateCommand ResetCommand { get; }
        DelegateCommand ReturnCommand { get; }

        void OnViewAttached(ITrainingParametersView view);

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ITrainingParametersController, TrainingParametersController>();
        }
    }


    internal class TrainingParametersController : ControllerBase<TrainingParametersViewModel>,ITrainingParametersController
    {
        private readonly AppState _appState;
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;
        private readonly AppStateHelper _helper;
        private bool _hasError;

        public TrainingParametersController(AppState appState, IRegionManager rm, IEventAggregator ea)
        {
            _appState = appState;
            _rm = rm;
            _ea = ea;
            _helper = new AppStateHelper(appState);

            OkCommand = new DelegateCommand(() =>
                {
                    var param = Vm!.TrainingParameters!.Clone();
                    _appState.ActiveSession!.TrainingParameters = param;
                    RaiseCommandsCanExec();
                    _hasError = false;
                },
                () => (!Vm!.View?.HasErrors() ?? true) && !Vm!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters));

            ResetCommand = new DelegateCommand(() =>
                {
                    var newParams = _appState.ActiveSession!.TrainingParameters!.Clone();
                    _hasError = false;
                    Vm!.TrainingParameters = newParams;
                    Vm!.CallParametersReseted();
                    AttachHandlersToParameters(newParams);
                    SetVmProperties();
                    RaiseCommandsCanExec();
                },
                () => !Vm!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters) || _hasError);


            ReturnCommand = new DelegateCommand(Return);
        }

        protected override void VmCreated()
        {
            Vm!.PropertyChanged += VmOnPropertyChanged;

            SetNewSession(_appState.ActiveSession!);
            _ea.GetEvent<EnableModalNavigation>().Publish(ReturnCommand);
        }



        private void ViewOnValidationError(object? sender, ValidationErrorEventArgs e)
        {
            if (e.Action != ValidationErrorEventAction.Removed)
            {
                _hasError = true;
                RaiseCommandsCanExec();
            }
            else
            {
                bool callRaise = _hasError;
                _hasError = Vm!.View!.HasErrors();
                if (!_hasError && callRaise)
                {
                    RaiseCommandsCanExec();
                }
            }
        }

        private void SetVmProperties()
        {
            Vm!.IsMaxLearningTimeChecked = Vm!.TrainingParameters!.MaxLearningTime == TimeSpan.MaxValue;
            Vm!.IsMaxEpochsChecked = Vm!.TrainingParameters.MaxEpochs == null;

            if (!Vm!.IsMaxLearningTimeChecked)
            {
                Vm!.MaxLearningTime = Time.Now.Add(Vm!.TrainingParameters.MaxLearningTime);
            }
        }

        private void SetNewSession(Session session)
        {
            Vm!.TrainingParameters = session.TrainingParameters!.Clone();
            AttachHandlersToParameters(Vm!.TrainingParameters);
            SetVmProperties();
        }

        private void AttachHandlersToParameters(TrainingParameters parameters)
        {
            parameters.PropertyChanged += (_, __) => RaiseCommandsCanExec();
            parameters.GDParams.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(GradientDescentParamsModel.BatchSize))
                {
                    var trainingSet = _appState.ActiveSession!.TrainingData!.Sets.TrainingSet;
                    if (trainingSet.Input.Count < parameters.GDParams.BatchSize)
                    {
                        throw new Exception($"Batch size ({parameters.GDParams.BatchSize}) must be less than or equal to {trainingSet.Input.Count}");
                    }

                    if (parameters.GDParams.BatchSize == trainingSet.Input.Count)
                    {
                        Vm!.TrainingParameters!.GDParams.Randomize = false;
                        Vm!.TrainingParameters!.CanRandomize = false;
                    }
                    else
                    {
                        Vm!.TrainingParameters!.CanRandomize = true;
                    }
                }

                RaiseCommandsCanExec();
            };
            parameters.LMParams.PropertyChanged += (_, __) => RaiseCommandsCanExec();
            Vm!.TrainingParameters!.CanRandomize = parameters.GDParams.BatchSize != _appState.ActiveSession!.TrainingData!.Sets.TrainingSet.Input.Count;
        }

        private void RaiseCommandsCanExec()
        {
            ResetCommand.RaiseCanExecuteChanged();
            OkCommand.RaiseCanExecuteChanged();
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (sender as TrainingParametersViewModel)!;
            switch (e.PropertyName)
            {
                case nameof(TrainingParametersViewModel.IsMaxLearningTimeChecked):
                    if (vm.IsMaxLearningTimeChecked)
                    {
                        Vm!.TrainingParameters!.MaxLearningTime = TimeSpan.MaxValue;
                    }
                    else
                    {
                        Vm!.MaxLearningTime = Time.Now.AddMinutes(10);
                        Vm!.TrainingParameters!.MaxLearningTime = Time.Now.AddMinutes(10) - Time.Now;
                    }

                    break;
                case nameof(TrainingParametersViewModel.MaxLearningTime):
                    Vm!.TrainingParameters!.MaxLearningTime = vm.MaxLearningTime - Time.Now;
                    break;
                case nameof(TrainingParametersViewModel.IsMaxEpochsChecked):
                    if (Vm!.IsMaxEpochsChecked)
                    {
                        Vm!.TrainingParameters!.MaxEpochs = null;
                    }
                    break;
            }
            RaiseCommandsCanExec();
        }

        private void Return()
        {
            _rm.NavigateContentRegion("TrainingView");
        }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand ResetCommand { get; }
        public DelegateCommand ReturnCommand { get; }
        public void OnViewAttached(ITrainingParametersView view)
        {
            view.ValidationError += ViewOnValidationError;
        }
    }
}