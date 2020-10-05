using System;
using System.ComponentModel;
using NNLib;
using Prism.Mvvm;

namespace Common.Domain
{
    public enum TrainingAlgorithm
    {
        GradientDescent,
        LevenbergMarquardt
    }

    public class GradientDescentParamsModel : BindableBase
    {
        private readonly GradientDescentParams _params;

        public GradientDescentParamsModel()
        {
            _params = new GradientDescentParams();
        }

        private GradientDescentParamsModel(GradientDescentParams para)
        {
            _params = para;
        }

        public GradientDescentParams Params => _params;


        public double LearningRate
        {
            get => _params.LearningRate;
            set
            {
                _params.LearningRate = value;
                RaisePropertyChanged();
            }
        }

        public double Momentum
        {
            get => _params.Momentum;
            set
            {
                _params.Momentum = value;
                RaisePropertyChanged();
            }
        }


        public int BatchSize
        {
            get => _params.BatchSize;
            set
            {
                _params.BatchSize = value;
                RaisePropertyChanged();
            }
        }

        public bool Equals(GradientDescentParamsModel obj)
        {
            return _params.Equals(obj._params);
        }

        public object Clone()
        {
            return new GradientDescentParamsModel((_params.Clone() as GradientDescentParams)!);
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

        public object Clone()
        {
            return new LevenbergMarquardtParamsModel((_params.Clone() as LevenbergMarquardtParams)!);
        }
    }

    public class TrainingParameters : BindableBase
    {
        private int _maxEpochs = int.MaxValue;
        private TimeSpan _maxLearningTime = TimeSpan.MaxValue;
        private double _targetError = 0.000001;
        private TrainingAlgorithm _algorithm = TrainingAlgorithm.GradientDescent;

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
            set => SetProperty(ref _targetError, value);
        }

        public TimeSpan MaxLearningTime
        {
            get => _maxLearningTime;
            set => SetProperty(ref _maxLearningTime, value);
        }

        public int MaxEpochs
        {
            get => _maxEpochs;
            set => SetProperty(ref _maxEpochs, value);
        }


        protected bool Equals(TrainingParameters other)
        {
            return _maxEpochs == other._maxEpochs && _maxLearningTime.Equals(other._maxLearningTime) && _targetError.Equals(other._targetError) && _algorithm == other._algorithm && GDParams.Equals(other.GDParams) && LMParams.Equals(other.LMParams);
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
            unchecked
            {
                var hashCode = _maxEpochs;
                hashCode = (hashCode * 397) ^ _maxLearningTime.GetHashCode();
                hashCode = (hashCode * 397) ^ _targetError.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) _algorithm;
                hashCode = (hashCode * 397) ^ GDParams.GetHashCode();
                hashCode = (hashCode * 397) ^ LMParams.GetHashCode();
                return hashCode;
            }
        }

        public TrainingParameters Clone()
        {
            return new TrainingParameters()
            {
                Algorithm = Algorithm, 
                GDParams = (GradientDescentParamsModel) GDParams.Clone(),
                LMParams = (LevenbergMarquardtParamsModel) LMParams.Clone(),
                MaxEpochs = MaxEpochs,
                MaxLearningTime = MaxLearningTime,
                TargetError = TargetError,
            };
        }
    }
}