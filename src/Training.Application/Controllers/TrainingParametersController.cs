using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Controls;
using Common.Domain;
using Common.Framework;
using Prism.Commands;
using Prism.Ioc;
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
    }
}

namespace Training.Application.Controllers
{
    internal class TrainingParametersController : ITransientController<TrainingParametersService>
    {
        private readonly IViewModelAccessor _accessor;
        private readonly AppState _appState;
        private TrainingParametersService _service = null!;

        public TrainingParametersController(IViewModelAccessor accessor, AppState appState)
        {
            _accessor = accessor;
            _appState = appState;

            _accessor.OnCreated<TrainingParametersViewModel>(() =>
            {
                var vm = _accessor.Get<TrainingParametersViewModel>()!;

                vm.PropertyChanged += VmOnPropertyChanged;

                _appState.ActiveSessionChanged += (sender, session) =>
                {
                    if (session.next.TrainingParameters != null)
                    {
                        vm.TrainingParameters = session.next.TrainingParameters.Clone();
                        AttachHandlersToParameters(vm.TrainingParameters);
                        vm.IsMaxLearningTimeChecked = vm.TrainingParameters.MaxLearningTime == TimeSpan.MaxValue;

                        if (!vm.IsMaxLearningTimeChecked)
                        {
                            vm.MaxLearningTime = Time.Now.Add(vm.TrainingParameters.MaxLearningTime);
                        }
                        else vm.MaxLearningTime = default;
                    }

                    session.next.PropertyChanged += (o, args) =>
                    {
                        if (args.PropertyName == nameof(Session.TrainingParameters))
                        {
                            vm.TrainingParameters = (o as Session)!.TrainingParameters!.Clone();
                            AttachHandlersToParameters(vm.TrainingParameters);
                            vm.IsMaxLearningTimeChecked = vm.TrainingParameters.MaxLearningTime == TimeSpan.MaxValue;

                            if (!vm.IsMaxLearningTimeChecked)
                            {
                                vm.MaxLearningTime = Time.Now.Add(vm.TrainingParameters.MaxLearningTime);
                            }
                            else vm.MaxLearningTime = default;
                        }
                    };
                };

                vm.TrainingParameters = _appState.ActiveSession?.TrainingParameters?.Clone();
                if (vm.TrainingParameters != null)
                {
                    AttachHandlersToParameters(vm.TrainingParameters);
                    vm.IsMaxLearningTimeChecked = vm.TrainingParameters.MaxLearningTime == TimeSpan.MaxValue;
                    if (!vm.IsMaxLearningTimeChecked)
                    {
                        vm.MaxLearningTime = Time.Now.Add(vm.TrainingParameters.MaxLearningTime);
                    }
                }
            });
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
                    var param = _accessor.Get<TrainingParametersViewModel>()!.TrainingParameters!.Clone();
                    _appState.ActiveSession!.TrainingParameters = param;
                    RaiseCommandsCanExec();
                },
                () => !_accessor.Get<TrainingParametersViewModel>()!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters));

            service.ResetCommand = new DelegateCommand(() =>
                {
                    var newParams = _appState.ActiveSession!.TrainingParameters!.Clone();
                    _accessor.Get<TrainingParametersViewModel>()!.TrainingParameters = newParams;
                    AttachHandlersToParameters(newParams);
                },
                () => !_accessor.Get<TrainingParametersViewModel>()!.TrainingParameters!
                    .Equals(_appState.ActiveSession!.TrainingParameters));
        }
    }
}