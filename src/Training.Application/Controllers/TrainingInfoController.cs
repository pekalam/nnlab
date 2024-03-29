﻿using Common.Framework;
using Prism.Events;
using System.ComponentModel;
using Training.Application.Plots;
using Training.Application.ViewModels;
using Training.Domain;

namespace Training.Application.Controllers
{
    public interface ITrainingInfoController : IController { }

    internal class TrainingInfoController : ControllerBase<TrainingInfoViewModel>,ITrainingInfoController
    {
        private readonly PlotEpochEndConsumer _epochEndConsumer;
        private readonly ModuleState _moduleState;

        public TrainingInfoController(IEventAggregator ea, ModuleState moduleState)
        {
            _moduleState = moduleState;
            _epochEndConsumer = new PlotEpochEndConsumer(moduleState,
                (args, session) =>
                {
                    var last = args[^1];
                    Vm!.View!.UpdateTraining(last.Error, last.Epoch, last.Iterations, last.ValidationError);
                },
                trainingSession =>
                {
                    Vm!.RestartTimer();
                },
                trainingSession =>
                {
                    var last = trainingSession.EpochEndEvents[^1];
                    Vm!.View!.UpdateTraining(last.Error, last.Epoch, last.Iterations, last.ValidationError);
                    Vm!.StopTimer();
                }, session =>
                {
                    var last = session.EpochEndEvents[^1];
                    Vm!.View!.UpdateTraining(last.Error, last.Epoch, last.Iterations, last.ValidationError);
                    Vm!.StopTimer();
                });



            _epochEndConsumer.Initialize();
        }

        protected override void VmCreated()
        {
            _moduleState.ActiveSessionChanged += (sender, args) =>
            {
                if (!args.next.StartTime.HasValue) Vm!.View!.ResetProgress();
                else
                {
                    var last = args.next.EpochEndEvents[^1];
                    Vm!.View!.UpdateTimer(args.next.CurrentReport!.Duration);
                    Vm!.View!.UpdateTraining(last.Error, last.Epoch, last.Iterations, last.ValidationError);
                }

                args.next.PropertyChanged -= SessionOnPropertyChanged;
                args.next.PropertyChanged += SessionOnPropertyChanged;
            };

        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TrainingSession.Started))
            {
                var session = (sender as TrainingSession)!;
                if (session.Started)
                {
                    Vm!.RestartTimer();
                }
                else
                {
                    Vm!.StopTimer();
                }
            }
        }

        public void Initialize()
        {
            
        }
    }
}
