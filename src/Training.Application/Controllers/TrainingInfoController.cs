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
        private readonly ModuleState _moduleState;

        public TrainingInfoController(IEventAggregator ea, IViewModelAccessor accessor, ModuleState moduleState) : base(accessor)
        {
            _moduleState = moduleState;
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

        protected override void VmCreated()
        {
            _moduleState.ActiveSessionChanged += (sender, args) =>
            {
                if (!args.next.StartTime.HasValue) Vm.View.ResetProgress();
                else
                {
                    var last = args.next.EpochEndEvents.Last();
                    Vm.View.UpdateTimer(args.next.CurrentReport.Duration);
                    Vm.View.UpdateTraining(last.Error, last.Epoch, last.Iterations);
                }

                args.next.PropertyChanged -= SessionOnPropertyChanged;
                args.next.PropertyChanged += SessionOnPropertyChanged;
            };

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
