using System;

namespace Common.Domain
{
    public class MaxEpochs : DomainPrimitive<int, MaxEpochs>
    {
        public MaxEpochs(int value) : base(value)
        {
            if (value <= 0 || value == int.MinValue)
            {
                throw new ArgumentException($"Invalid maxepochs value: {value}");
            }
        }

        public bool IsUndefined => Value == int.MaxValue;

        public static implicit operator int(MaxEpochs v) => v.Value;
        public static implicit operator MaxEpochs(int v) => new MaxEpochs(v);
    }
}