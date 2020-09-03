using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using Common.Domain;
using Common.Framework;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Shell.Interface;
using Training.Application.Controllers;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Services
{
    public interface ITrainingParametersService : IService
    {
        DelegateCommand OkCommand { get; }
        DelegateCommand ResetCommand { get; }
        DelegateCommand ReturnCommand { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ITrainingParametersService, TrainingParametersService>()
                .Register<ITransientController<TrainingParametersService>, TrainingParametersController>();
        }
    }

    internal class TrainingParametersService : ITrainingParametersService
    {
        public TrainingParametersService(ITransientController<TrainingParametersService> ctrl)
        {
            ctrl.Initialize(this);
        }

        public DelegateCommand OkCommand { get; set; } = null!;
        public DelegateCommand ResetCommand { get; set; } = null!;
        public DelegateCommand ReturnCommand { get; set; } = null!;
    }
}

namespace Training.Application.Controllers
{
    internal class TrainingParametersController : ControllerBase<TrainingParametersViewModel>,ITransientController<TrainingParametersService>
    {
        private readonly AppState _appState;
        private TrainingParametersService _service = null!;
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;

        public TrainingParametersController(IViewModelAccessor accessor, AppState appState, IRegionManager rm, IEventAggregator ea) : base(accessor)
        {
            _appState = appState;
            _rm = rm;
            _ea = ea;
        }

        protected override void VmCreated()
        {
            Vm!.PropertyChanged += VmOnPropertyChanged;

            _appState.ActiveSessionChanged += (sender, session) =>
            {
                if (session.next.TrainingParameters != null)
                {
                    Vm!.TrainingParameters = session.next.TrainingParameters.Clone();
                    AttachHandlersToParameters(Vm!.TrainingParameters);
                    Vm!.IsMaxLearningTimeChecked = Vm!.TrainingParameters.MaxLearningTime == TimeSpan.MaxValue;

                    if (!Vm!.IsMaxLearningTimeChecked)
                    {
                        Vm!.MaxLearningTime = Time.Now.Add(Vm!.TrainingParameters.MaxLearningTime);
                    }
                    else Vm!.MaxLearningTime = default;
                }

                session.next.PropertyChanged += (o, args) =>
                {
                    if (args.PropertyName == nameof(Session.TrainingParameters))
                    {
                        Vm!.TrainingParameters = (o as Session)!.TrainingParameters!.Clone();
                        AttachHandlersToParameters(Vm!.TrainingParameters);
                        Vm!.IsMaxLearningTimeChecked = Vm!.TrainingParameters.MaxLearningTime == TimeSpan.MaxValue;

                        if (!Vm!.IsMaxLearningTimeChecked)
                        {
                            Vm!.MaxLearningTime = Time.Now.Add(Vm!.TrainingParameters.MaxLearningTime);
                        }
                        else Vm!.MaxLearningTime = default;
                    }
                };
            };

            Vm!.TrainingParameters = _appState.ActiveSession?.TrainingParameters?.Clone();
            if (Vm!.TrainingParameters != null)
            {
                AttachHandlersToParameters(Vm!.TrainingParameters);
                Vm!.IsMaxLearningTimeChecked = Vm!.TrainingParameters.MaxLearningTime == TimeSpan.MaxValue;
                if (!Vm!.IsMaxLearningTimeChecked)
                {
                    Vm!.MaxLearningTime = Time.Now.Add(Vm!.TrainingParameters.MaxLearningTime);
                }
            }


            _ea.GetEvent<EnableModalNavigation>().Publish(_service.ReturnCommand);
        }

        private void AttachHandlersToParameters(TrainingParameters parameters)
        {
            parameters.PropertyChanged += (_, __) => RaiseCommandsCanExec();
            parameters.GDParams.PropertyChanged += (_, __) => RaiseCommandsCanExec();
            parameters.LMParams.PropertyChanged += (_, __) => RaiseCommandsCanExec();
        }

        private void RaiseCommandsCanExec()
        {
            _service.ResetCommand.RaiseCanExecuteChanged();
            _service.OkCommand.RaiseCanExecuteChanged();
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

        public void Initialize(TrainingParametersService service)
        {
            _service = service;
            service.OkCommand = new DelegateCommand(() =>
                {
                    var param = Vm!.TrainingParameters!.Clone();
                    _appState.ActiveSession!.TrainingParameters = param;
                    RaiseCommandsCanExec();
                },
                () => !Vm!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters));

            service.ResetCommand = new DelegateCommand(() =>
                {
                    var newParams = _appState.ActiveSession!.TrainingParameters!.Clone();
                    Vm!.TrainingParameters = newParams;
                    AttachHandlersToParameters(newParams);
                },
                () => !Vm!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters));


            service.ReturnCommand = new DelegateCommand(Return);
        }

        private void Return()
        {
            _rm.NavigateContentRegion("TrainingView");
        }
    }
}