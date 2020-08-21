using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MathNet.Numerics.LinearAlgebra;

namespace SharedUI.MatrixPreview
{
    public class MatrixRowDictionary : IDictionary<int, string>
    {
        private readonly IDictionary<int, string> _dictionary = new Dictionary<int, string>();
        private Matrix<double>? _matrix;
        private int _row;

        public void AssignMatrix(Matrix<double> matrix, int row)
        {
            _matrix = matrix;
            _row = row;
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _dictionary).GetEnumerator();

        public void Add(KeyValuePair<int, string> item) => _dictionary.Add(item);

        public void Clear() => _dictionary.Clear();

        public bool Contains(KeyValuePair<int, string> item) => _dictionary.Contains(item);

        public void CopyTo(KeyValuePair<int, string>[] array, int arrayIndex) => _dictionary.CopyTo(array, arrayIndex);

        public bool Remove(KeyValuePair<int, string> item) => _dictionary.Remove(item);

        public int Count => _dictionary.Count;

        public bool IsReadOnly => _dictionary.IsReadOnly;

        public void Add(int key, string value) => _dictionary.Add(key, value);

        public bool ContainsKey(int key) => _dictionary.ContainsKey(key);

        public bool Remove(int key) => _dictionary.Remove(key);

        public bool TryGetValue(int key, out string value) => _dictionary.TryGetValue(key, out value);

        public string this[int key]
        {
            get => _dictionary[key];
            set
            {
                if (_matrix != null)
                {
                    _matrix[_row, key] = double.Parse(value);
                }
                _dictionary[key] = value;
            }
        }

        public ICollection<int> Keys => _dictionary.Keys;

        public ICollection<string> Values => _dictionary.Values;
    }

    public class MatrixPreviewModel : INotifyPropertyChanged
    {
        public string RowHeader { get; set; }
        public MatrixRowDictionary Props { get; set; }
        public void RaisePropsChanged() => OnPropertyChanged(nameof(Props));
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}