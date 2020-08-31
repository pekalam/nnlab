using System;

namespace Common.Domain
{
    public class DomainPrimitive<TVal, TSubcl> : IEquatable<TSubcl>, IComparable<TSubcl>
        where TSubcl : DomainPrimitive<TVal, TSubcl> where TVal : IComparable<TVal>
    {
        public TVal Value { get; }

        public DomainPrimitive(TVal value)
        {
            Value = value;
        }

        public bool Equals(TSubcl? other)
        {
            if (other == null)
                return false;
            return Value.Equals(other.Value);
        }

        public int CompareTo(TSubcl? other)
        {
            if (other == null)
                return 1;
            return Value.CompareTo(other.Value);
        }

        public override bool Equals(object? obj) => (obj is TSubcl) && Equals(obj as TSubcl);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value!.ToString()!;
    }
}