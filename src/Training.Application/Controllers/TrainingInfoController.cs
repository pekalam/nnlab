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
using Training.Application.Plots;
using Training.Application.ViewModels;
using Training.Domain;
using Training.Interface;

namespace Training.Application.Controllers
{
    internal interface ITrainingInfoController : ISingletonController { }

    class TrainingInfoController : ControllerBase<TrainingInfoViewModel>,ITrainingInfoController
    {
        private readonly PlotEpochEndConsumer _epochEndConsumer;

        public TrainingInfoController(IEventAggregator ea, IViewModelAccessor accessor, ModuleState moduleState) : base(accessor)
        {
            _epochEndConsumer = new PlotEpochEndConsumer(moduleState,
                (args, session) =>
                {

                    var last = args.Last();
                    Vm.View.UpdateTraining(last.Error, last.Epoch, last.Iterations);
                },
                trainingSession =>
                {
                    Vm.StartTimer();
                    Vm.TestError = null;
                    Vm.ValidationError = null;
                },
                trainingSession =>
                {
                    Vm.StopTimer();
                }, session =>
                {
                    Vm.StopTimer();
                });

            moduleState.ActiveSessionChanged += (sender, args) =>
            {
                args.next.PropertyChanged += SessionOnPropertyChanged;
            };

            ea.GetEvent<TrainingValidationFinished>().Subscribe(d =>
            {
                Vm.ValidationError = d;
            });

            ea.GetEvent<TrainingTestFinished>().Subscribe(d =>
            {
                Vm.TestError = d;
            });


            _epochEndConsumer.Initialize();
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingSession.Started))
            {
                var session = sender as TrainingSession;
                if (session.Started)
                {
                    Vm.StartTimer();
                }
                else
                {
                    Vm.StopTimer();
                }
            }
        }

        public void Initialize()
        {
            
        }
    }
}
