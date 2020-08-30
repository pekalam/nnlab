using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Common.Domain;
using Common.Framework;
using Prism.Events;
using Prism.Regions;
using Training.Application.ViewModels;
using Training.Domain;
using Training.Interface;

namespace Training.Application.Controllers
{
    internal interface ITrainingInfoController : ISingletonController { }

    class TrainingInfoController : ITrainingInfoController
    {
        private readonly PlotEpochEndConsumer _epochEndConsumer;
        private readonly IViewModelAccessor _accessor;

        public TrainingInfoController(IEventAggregator ea, IViewModelAccessor accessor, ModuleState moduleState)
        {
            _accessor = accessor;
            _epochEndConsumer = new PlotEpochEndConsumer(moduleState,
                (args, session) =>
                {
                    var vm = _accessor.Get<TrainingInfoViewModel>();

                    var last = args.Last();
                    vm.View.UpdateTraining(last.Error, last.Epoch, last.Iterations);
                },
                trainingSession =>
                {
                    var vm = _accessor.Get<TrainingInfoViewModel>();
                    vm.StartTimer();
                    vm.TestError = null;
                    vm.ValidationError = null;
                },
                trainingSession =>
                {
                    _accessor.Get<TrainingInfoViewModel>().StopTimer();
                }, session =>
                {
                    _accessor.Get<TrainingInfoViewModel>().StopTimer();
                });

            moduleState.ActiveSessionChanged += (sender, args) =>
            {
                args.next.PropertyChanged += SessionOnPropertyChanged;
            };

            ea.GetEvent<TrainingValidationFinished>().Subscribe(d =>
            {
                _accessor.Get<TrainingInfoViewModel>().ValidationError = d;
            });

            ea.GetEvent<TrainingTestFinished>().Subscribe(d =>
            {
                _accessor.Get<TrainingInfoViewModel>().TestError = d;
            });


            _epochEndConsumer.Initialize();
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingSession.Started))
            {
                var session = sender as TrainingSession;
                var vm = _accessor.Get<TrainingInfoViewModel>();
                if (session.Started)
                {
                    vm.StartTimer();
                }
                else
                {
                    vm.StopTimer();
                }
            }
        }

        public void Initialize()
        {
            
        }
    }
}
