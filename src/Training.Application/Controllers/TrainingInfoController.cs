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
        private PlotEpochEndConsumer _epochEndConsumer;
        private IViewModelAccessor _accessor;

        public TrainingInfoController(IEventAggregator ea, IViewModelAccessor accessor, ModuleState moduleState)
        {
            _accessor = accessor;
            _epochEndConsumer = new PlotEpochEndConsumer(moduleState,
                (args, session) =>
                {
                    var last = args.Last();
                    _accessor.Get<TrainingInfoViewModel>().UpdateTraining(last.Error, last.Epoch, last.Iterations);
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

            moduleState.ActiveSessionChanged += (sender, session) =>
            {
                session.PropertyChanged += SessionOnPropertyChanged;
            };

            ea.GetEvent<TrainingValidationFinished>().Subscribe(d =>
            {
                _accessor.Get<TrainingInfoViewModel>().ValidationError = d;
            });

            ea.GetEvent<TrainingTestFinished>().Subscribe(d =>
            {
                _accessor.Get<TrainingInfoViewModel>().TestError = d;
            });
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingSession.Started))
            {
                var session = sender as TrainingSession;
                if (session.Started)
                {
                    _accessor.Get<TrainingInfoViewModel>().StartTime = session.StartTime;
                }
            }
        }

        public void Initialize()
        {
            
        }
    }
}
