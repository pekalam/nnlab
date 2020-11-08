using System;
using Common.Domain;
using Common.Framework;
using Training.Application.Controllers;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingParametersViewModel : ViewModelBase<TrainingParametersViewModel>
    {
        private TrainingParameters? _trainingParameters;
        private bool _isMaxLearningTimeChecked;
        private DateTime _maxLearningTime;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public TrainingParametersViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        }

        [InjectionConstructor]
        public TrainingParametersViewModel(ITrainingParametersController service)
        {
            Service = service;
            service.Initialize(this);
        }

        public ITrainingParametersController Service { get; }


        public TrainingParameters? TrainingParameters
        {
            get => _trainingParameters;
            set => SetProperty(ref _trainingParameters, value);
        }

        public TrainingAlgorithm[] Algorithms { get; } = new TrainingAlgorithm[]
        {
            TrainingAlgorithm.GradientDescent, TrainingAlgorithm.LevenbergMarquardt
        };

        public bool IsMaxLearningTimeChecked
        {
            get => _isMaxLearningTimeChecked;
            set => SetProperty(ref _isMaxLearningTimeChecked, value);
        }

        public DateTime MaxLearningTime
        {
            get => _maxLearningTime;
            set => SetProperty(ref _maxLearningTime, value);
        }
    }
}