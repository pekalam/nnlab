using System;

namespace Infrastructure.Domain
{
    public class TargetError : DomainPrimitive<double, TargetError>
    {
        public TargetError(double value) : base(value)
        {
            if ((!double.IsFinite(value) && !double.IsPositiveInfinity(value)) || value <= 0)
            {
                throw new ArgumentException($"Invalid target error: {value}");
            }
        }

        public bool IsUndefined => double.IsPositiveInfinity(Value);

        public static implicit operator double(TargetError v) => v.Value;
        public static implicit operator TargetError(double v) => new TargetError(v);
    }
}