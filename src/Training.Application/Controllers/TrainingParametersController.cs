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
using Training.Application.ViewModels;
using Training.Application.Views;

namespace Training.Application.Controllers
{
    public interface ITrainingParametersController : ITransientController
    {
        DelegateCommand OkCommand { get; }
        DelegateCommand ResetCommand { get; }
        DelegateCommand ReturnCommand { get; }

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
                },
                () => (!Vm!.View?.HasErrors() ?? true) && !Vm!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters));

            ResetCommand = new DelegateCommand(() =>
                {
                    var newParams = _appState.ActiveSession!.TrainingParameters!.Clone();
                    Vm!.TrainingParameters = newParams;
                    AttachHandlersToParameters(newParams);
                },
                () => !Vm!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters));


            ReturnCommand = new DelegateCommand(Return);
        }

        protected override void VmCreated()
        {
            Vm!.PropertyChanged += VmOnPropertyChanged;


            _helper.OnSessionChangedOrSet(session =>
            {
                if (session.TrainingParameters != null)
                {
                    SetNewSession(session);
                }

                session.PropertyChanged += SessionOnPropertyChanged;
            });

            _ea.GetEvent<EnableModalNavigation>().Publish(ReturnCommand);
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Session.TrainingParameters))
            {
                SetNewSession((sender as Session)!);
            }
        }

        private void SetNewSession(Session session)
        {
            Vm!.TrainingParameters = session.TrainingParameters!.Clone();
            AttachHandlersToParameters(Vm!.TrainingParameters);
            Vm!.IsMaxLearningTimeChecked = Vm!.TrainingParameters.MaxLearningTime == TimeSpan.MaxValue;
            Vm!.IsMaxEpochsChecked = Vm!.TrainingParameters.MaxEpochs == null;

            if (!Vm!.IsMaxLearningTimeChecked)
            {
                Vm!.MaxLearningTime = Time.Now.Add(Vm!.TrainingParameters.MaxLearningTime);
            }
            else Vm!.MaxLearningTime = default;
        }

        private void AttachHandlersToParameters(TrainingParameters parameters)
        {
            parameters.PropertyChanged += (_, __) => RaiseCommandsCanExec();
            parameters.GDParams.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(GradientDescentParamsModel.BatchSize))
                {
                    var trainingSet = _appState.ActiveSession!.TrainingData!.Sets.TrainingSet;
                    if (trainingSet.Input.Count % parameters.GDParams.BatchSize != 0)
                    {
                        throw new Exception($"Number of training examples ({trainingSet.Input.Count}) is not divisible by {parameters.GDParams.BatchSize}");
                    }
                }

                RaiseCommandsCanExec();
            };
            parameters.LMParams.PropertyChanged += (_, __) => RaiseCommandsCanExec();
        }

        private void RaiseCommandsCanExec()
        {
            ResetCommand.RaiseCanExecuteChanged();
            OkCommand.RaiseCanExecuteChanged();
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (sender as TrainingParametersViewModel)!;
            RaiseCommandsCanExec();
            switch (e.PropertyName)
            {
                case nameof(TrainingParametersViewModel.IsMaxLearningTimeChecked):
                    if (vm.IsMaxLearningTimeChecked)
                    {
                        _appState.ActiveSession!.TrainingParameters!.MaxLearningTime = TimeSpan.MaxValue;
                    }
                    else
                    {
                        _appState.ActiveSession!.TrainingParameters!.MaxLearningTime = Time.Now.AddMinutes(10) - Time.Now;
                    }

                    break;
                case nameof(TrainingParametersViewModel.MaxLearningTime):
                    _appState.ActiveSession!.TrainingParameters!.MaxLearningTime = vm.MaxLearningTime - Time.Now;
                    break;
            }
        }

        private void Return()
        {
            _rm.NavigateContentRegion("TrainingView");
        }

        public DelegateCommand OkCommand { get; }
        public DelegateCommand ResetCommand { get; }
        public DelegateCommand ReturnCommand { get; }
    }
}