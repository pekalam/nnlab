using System;
using System.ComponentModel;
using NNLib;

namespace Common.Domain
{
    public enum TrainingAlgorithm
    {
        GradientDescent,
        LevenbergMarquardt
    }


    public class TrainingParameters : IDataErrorInfo
    {
        public GradientDescentParams GDParams { get; set; } = new GradientDescentParams();
        public LevenbergMarquardtParams LMParams { get; set; } = new LevenbergMarquardtParams();
        public TrainingAlgorithm Algorithm { get; set; } = TrainingAlgorithm.GradientDescent;
        public double TargetError { get; set; } = 0.000001;
        public TimeSpan MaxLearningTime { get; set; } = TimeSpan.MaxValue;
        public int MaxEpochs { get; set; } = int.MaxValue;

        // public override bool Equals(object? obj)
        // {
        //     if (obj == null)
        //         return false;
        //     if (obj is TrainingParameters o)
        //     {
        //         return GDParams.Equals(o.GDParams) && LMParams.Equals(o.LMParams) && Algorithm.Equals(o.Algorithm) &&
        //                TargetError.Equals(o.TargetError) && MaxLearningTime.Equals(o.MaxLearningTime) &&
        //                MaxEpochs.Equals(o.MaxEpochs);
        //     }
        //
        //     return false;
        // }

        public TrainingParameters Clone()
        {
            return new TrainingParameters()
            {
                Algorithm = Algorithm, 
                GDParams = (GradientDescentParams) GDParams.Clone(),
                LMParams = (LevenbergMarquardtParams) LMParams.Clone(),
                MaxEpochs = MaxEpochs,
                MaxLearningTime = MaxLearningTime,
                TargetError = TargetError,
            };
        }

        public string Error { get; }

        public string this[string columnName]
        {
            get
            {
                return null;
            }
        }
    }
}