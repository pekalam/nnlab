using NNLib.Training.GradientDescent;
using NNLib.Training.LevenbergMarquardt;
using Prism.Mvvm;
using System;
using System.Collections;
using System.ComponentModel;

namespace Common.Domain
{
    public enum TrainingAlgorithm
    {
        GradientDescent,
        LevenbergMarquardt
    }

    public class GradientDescentParamsModel : BindableBase
    {
        public GradientDescentParamsModel()
        {
            Params = new GradientDescentParams();
        }

        private GradientDescentParamsModel(GradientDescentParams para)
        {
            Params = para;
        }

        public GradientDescentParams Params { get; }


        public double LearningRate
        {
            get => Params.LearningRate;
            set
            {
                Params.LearningRate = value;
                RaisePropertyChanged();
            }
        }

        public double Momentum
        {
            get => Params.Momentum;
            set
            {
                Params.Momentum = value;
                RaisePropertyChanged();
            }
        }


        public int BatchSize
        {
            get => Params.BatchSize;
            set
            {
                Params.BatchSize = value;
                RaisePropertyChanged();
            }
        }

        public bool Equals(GradientDescentParamsModel obj)
        {
            return Params.Equals(obj.Params);
        }

        public GradientDescentParamsModel Clone()
        {
            return new GradientDescentParamsModel((Params.Clone() as GradientDescentParams)!);
        }
    }

    public class LevenbergMarquardtParamsModel : BindableBase
    {
        private readonly LevenbergMarquardtParams _params;

        private LevenbergMarquardtParamsModel(LevenbergMarquardtParams @params)
        {
            _params = @params;
        }

        public LevenbergMarquardtParamsModel()
        {
            _params = new LevenbergMarquardtParams();
        }

        public LevenbergMarquardtParams Params => _params;

        public double DampingParamIncFactor
        {
            get => _params.DampingParamIncFactor;
            set
            {
                _params.DampingParamIncFactor = value;
                RaisePropertyChanged();
            }
        }

        public double DampingParamDecFactor
        {
            get => _params.DampingParamDecFactor;
            set
            {
                _params.DampingParamDecFactor = value;
                RaisePropertyChanged();
            }
        }

        public bool Equals(LevenbergMarquardtParamsModel obj)
        {
            return _params.Equals(obj._params);
        }

        public LevenbergMarquardtParamsModel Clone()
        {
            return new LevenbergMarquardtParamsModel((_params.Clone() as LevenbergMarquardtParams)!);
        }
    }

    public class TrainingParameters : BindableBase
    {
        private int? _maxEpochs;
        private TimeSpan _maxLearningTime = TimeSpan.MaxValue;
        private double _targetError = 0.000001;
        private TrainingAlgorithm _algorithm = TrainingAlgorithm.GradientDescent;
        private bool _runValidation;
        private int _validationEpochThreshold = 1;
        private bool _addReportOnPause = true;
        private bool _canRunValidation;
        private bool _stopWhenValidationErrorReached;
        private double _validationTargetError;

        public TrainingParameters(bool canRunValidation)
        {
            CanRunValidation = canRunValidation;
            if (CanRunValidation) RunValidation = true;
        }

        public GradientDescentParamsModel GDParams { get; set; } = new GradientDescentParamsModel();
        public LevenbergMarquardtParamsModel LMParams { get; set; } = new LevenbergMarquardtParamsModel();

        public TrainingAlgorithm Algorithm
        {
            get => _algorithm;
            set => SetProperty(ref _algorithm, value);
        }

        public double TargetError
        {
            get => _targetError;
            set
            {
                if (value < 0) throw new ArgumentException("Target error must be grater than zero");
                SetProperty(ref _targetError, value);
            }
        }

        public TimeSpan MaxLearningTime
        {
            get => _maxLearningTime;
            set => SetProperty(ref _maxLearningTime, value);
        }

        public int? MaxEpochs
        {
            get => _maxEpochs;
            set
            {
                if (value < 0) throw new ArgumentException("Max epochs must be grater than zero");
                SetProperty(ref _maxEpochs, value);
            }
        }

        public bool RunValidation
        {
            get => _runValidation;
            set
            {
                if (!CanRunValidation && value)
                    throw new ArgumentException(
                        $"Cannot set {nameof(RunValidation)} to true if {nameof(CanRunValidation)}=false");
                SetProperty(ref _runValidation, value);
            }
        }

        public int ValidationEpochThreshold
        {
            get => _validationEpochThreshold;
            set
            {
                if (value < 0) throw new ArgumentException("Validation epoch threshold must be grater than zero");
                SetProperty(ref _validationEpochThreshold, value);
            }
        }

        public bool AddReportOnPause
        {
            get => _addReportOnPause;
            set => SetProperty(ref _addReportOnPause, value);
        }

        public bool CanRunValidation
        {
            get => _canRunValidation;
            internal set
            {
                SetProperty(ref _canRunValidation, value);
                if (value) RunValidation = true;
            }
        }

        public bool StopWhenValidationErrorReached
        {
            get => _stopWhenValidationErrorReached;
            set => SetProperty(ref _stopWhenValidationErrorReached, value);
        }

        public double ValidationTargetError
        {
            get => _validationTargetError;
            set
            {
                if (value < 0) throw new ArgumentException("Validation target error must be grater than zero");
                SetProperty(ref _validationTargetError, value);
            }
        }

        protected bool Equals(TrainingParameters other)
        {
            return _maxEpochs == other._maxEpochs && _maxLearningTime.Equals(other._maxLearningTime) &&
                   _targetError.Equals(other._targetError) && _algorithm == other._algorithm &&
                   _runValidation == other._runValidation &&
                   _validationEpochThreshold == other._validationEpochThreshold &&
                   _addReportOnPause == other._addReportOnPause && _canRunValidation == other._canRunValidation &&
                   _stopWhenValidationErrorReached == other._stopWhenValidationErrorReached &&
                   _validationTargetError == other._validationTargetError && GDParams.Equals(other.GDParams) &&
                   LMParams.Equals(other.LMParams);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TrainingParameters) obj);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(_maxEpochs);
            hashCode.Add(_maxLearningTime);
            hashCode.Add(_targetError);
            hashCode.Add((int) _algorithm);
            hashCode.Add(_runValidation);
            hashCode.Add(_validationEpochThreshold);
            hashCode.Add(_addReportOnPause);
            hashCode.Add(_canRunValidation);
            hashCode.Add(_stopWhenValidationErrorReached);
            hashCode.Add(_validationTargetError);
            hashCode.Add(GDParams);
            hashCode.Add(LMParams);
            return hashCode.ToHashCode();
        }

        public TrainingParameters Clone()
        {
            return new TrainingParameters(this.CanRunValidation)
            {
                GDParams = GDParams.Clone(), LMParams = LMParams.Clone(),
                Algorithm = Algorithm,
                AddReportOnPause = AddReportOnPause,
                MaxEpochs = MaxEpochs,
                MaxLearningTime = MaxLearningTime,
                RunValidation = RunValidation,
                TargetError = TargetError,
                ValidationEpochThreshold = ValidationEpochThreshold,
                StopWhenValidationErrorReached = StopWhenValidationErrorReached,
                ValidationTargetError = ValidationTargetError,
            };
        }
    }
}