using System;
using Common.Domain;
using Common.Framework;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingParametersViewModel : ViewModelBase<TrainingParametersViewModel>
    {
        private TrainingParameters _trainingParameters;
        private bool _isMaxLearningTimeChecked = true;
        private DateTime _maxLearningTime;

        public TrainingParametersViewModel()
        {
            
        }

        [InjectionConstructor]
        public TrainingParametersViewModel(AppState appState)
        {
            appState.ActiveSessionChanged += (sender, session) =>
                TrainingParameters = appState.ActiveSession.TrainingParameters;
            TrainingParameters = appState.ActiveSession?.TrainingParameters;
        }


        public TrainingParameters TrainingParameters
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
            set
            {
                _isMaxLearningTimeChecked = value;
                if (!value)
                {
                    TrainingParameters.MaxLearningTime = TimeSpan.MaxValue;
                }
                else
                {
                    TrainingParameters.MaxLearningTime = DateTime.Now.AddMinutes(10) - DateTime.Now;
                }
            }
        }

        public DateTime MaxLearningTime
        {
            get => _maxLearningTime;
            set
            {
                SetProperty(ref _maxLearningTime, value);
                TrainingParameters.MaxLearningTime = DateTime.Now - value;
            }
        }
    }
}