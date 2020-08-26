using System;
using Common.Domain;
using Common.Framework;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingParametersViewModel : ViewModelBase<TrainingParametersViewModel>
    {
        private TrainingParameters _trainingParameters;
        private bool _isMaxLearningTimeChecked;
        private DateTime _maxLearningTime;

        public TrainingParametersViewModel()
        {
        }

        [InjectionConstructor]
        public TrainingParametersViewModel(AppState appState)
        {
            //TODO ctrl
            appState.ActiveSessionChanged += (sender, session) =>
            {
                if (session.next.TrainingParameters != null)
                {
                    TrainingParameters = session.next.TrainingParameters;
                    _isMaxLearningTimeChecked = _trainingParameters.MaxLearningTime == TimeSpan.MaxValue;
                    RaisePropertyChanged(nameof(IsMaxLearningTimeChecked));

                    if (!_isMaxLearningTimeChecked)
                    {
                        _maxLearningTime = Time.Now.Add(_trainingParameters.MaxLearningTime);
                        RaisePropertyChanged(nameof(MaxLearningTime));
                    }
                    else _maxLearningTime = default;
                }

                session.next.PropertyChanged += (o, args) =>
                {
                    if (args.PropertyName == nameof(Session.TrainingParameters))
                    {
                        TrainingParameters = (o as Session).TrainingParameters;
                        _isMaxLearningTimeChecked = _trainingParameters.MaxLearningTime == TimeSpan.MaxValue;
                        RaisePropertyChanged(nameof(IsMaxLearningTimeChecked));

                        if (!_isMaxLearningTimeChecked)
                        {
                            _maxLearningTime = Time.Now.Add(_trainingParameters.MaxLearningTime);
                            RaisePropertyChanged(nameof(MaxLearningTime));
                        }
                        else _maxLearningTime = default;
                    }
                };
            };

            TrainingParameters = appState.ActiveSession?.TrainingParameters;
            if (_trainingParameters != null)
            {
                _isMaxLearningTimeChecked = _trainingParameters.MaxLearningTime == TimeSpan.MaxValue;
                if (!_isMaxLearningTimeChecked)
                {
                    _maxLearningTime = Time.Now.Add(_trainingParameters.MaxLearningTime);
                }

            }

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
                if (value)
                {
                    TrainingParameters.MaxLearningTime = TimeSpan.MaxValue;
                }
                else
                {
                    TrainingParameters.MaxLearningTime = Time.Now.AddMinutes(10) - Time.Now;
                }
            }
        }

        public DateTime MaxLearningTime
        {
            get => _maxLearningTime;
            set
            {
                SetProperty(ref _maxLearningTime, value);
                TrainingParameters.MaxLearningTime = value - Time.Now;
            }
        }
    }
}