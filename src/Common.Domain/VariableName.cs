namespace Common.Domain
{
    public class VariableName
    {
        private readonly string _value;

        public VariableName(string value)
        {
            if (value.Length == 0)
            {
                value = "Unknown";
            }

            _value = value;
        }

        protected bool Equals(VariableName other)
        {
            return string.Equals(_value, other._value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VariableName) obj);
        }

        public override int GetHashCode()
        {
            return (_value != null ? _value.GetHashCode() : 0);
        }

        public static implicit operator string(VariableName v) => v._value;
        public static implicit operator VariableName(string v) => new VariableName(v);

        public override string ToString()
        {
            return _value;
        }
    }


    public static class ModuleIds
    {
        public const int Data = 1;
        public const int NeuralNetwork = 2;
        public const int Training = 3;
        public const int Shell = 4;
        public const int Prediction = 5;
    }
}