using System;

namespace Infrastructure.Domain
{
    public class MaxLearningTime : DomainPrimitive<TimeSpan, MaxLearningTime>
    {
        public MaxLearningTime(TimeSpan value) : base(value)
        {
            if (value.Ticks <= 0)
            {
                throw new ArgumentException("Invalid timeSpan value");
            }

            if (value == TimeSpan.MinValue)
            {
                throw new ArgumentException("Invalid timeSpan value");
            }
        }

        public bool IsUndefined => Value == TimeSpan.MaxValue;

        public static implicit operator TimeSpan(MaxLearningTime v) => v.Value;
        public static implicit operator MaxLearningTime(TimeSpan v) => new MaxLearningTime(v);
    }
}