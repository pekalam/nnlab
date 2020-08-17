using System;
using NNLib;
using Prism.Mvvm;

namespace Infrastructure.Domain
{
    public enum TrainingAlgorithm
    {
        GradientDescent,
        LevenbergMarquardt
    }


    public class TrainingParameters
    {
        public GradientDescentParams GDParams { get; set; } = new GradientDescentParams();
        public LevenbergMarquardtParams LMParams { get; set; } = new LevenbergMarquardtParams();
        public TrainingAlgorithm Algorithm { get; set; } = TrainingAlgorithm.GradientDescent;
        public double TargetError { get; set; } = 0.000001;
        public TimeSpan MaxLearningTime { get; set; } = TimeSpan.MaxValue;
        public int MaxEpochs { get; set; } = int.MaxValue;

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;
            if (obj is TrainingParameters o)
            {
                return GDParams.Equals(o.GDParams) && LMParams.Equals(o.LMParams) && Algorithm.Equals(o.Algorithm) &&
                       TargetError.Equals(o.TargetError) && MaxLearningTime.Equals(o.MaxLearningTime) &&
                       MaxEpochs.Equals(o.MaxEpochs);
            }

            return false;
        }
    }
}