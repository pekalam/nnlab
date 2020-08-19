using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using MathNet.Numerics.LinearAlgebra;

namespace CommonUI.MatrixPreview
{
    internal class MatrixGridRenderer
    {
        private readonly MatrixPreviewViewModel _vm;
        private List<MatrixPreviewModel> _models;
        private readonly List<DataGridTextColumn> _columns = new List<DataGridTextColumn>();

        public MatrixGridRenderer(MatrixPreviewViewModel vm)
        {
            _vm = vm;
        }

        public bool ReadOnly { get; set; } = true;

        public bool Rendered => _models != null;

        public void Create(Matrix<double> matrix, string format, Func<int, string> columnTitle,
            Func<int, string> rowTitle)
        {
            _columns.Clear();
            lock (_vm)
            {
                _models = new List<MatrixPreviewModel>();

                for (int i = 0; i < matrix.ColumnCount; i++)
                {
                    _columns.Add(new DataGridTextColumn()
                    {
                        Binding = new Binding("Props[" + i + "]")
                        {
                            Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            ValidationRules = { new DoubleValidationRule()}
                        },
                        Header = columnTitle(i),
                    });
                }

                for (int i = 0; i < matrix.RowCount; i++)
                {
                    var model = new MatrixPreviewModel
                    {
                        Props = new MatrixRowDictionary(),
                        RowHeader = rowTitle(i),
                    };

                    for (int j = 0; j < matrix.ColumnCount; j++)
                    {
                        model.Props[j] = matrix[i, j].ToString(format);
                    }

                    if (!ReadOnly)
                    {
                        model.Props.AssignMatrix(matrix, i);
                    }

                    _models.Add(model);
                }

                _vm.UpdateColumns(_columns);
                _vm.Source = _models;
            }
        }

        public void Update(Matrix<double> matrix, string format)
        {
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    _models[i].Props[j] = matrix[i, j].ToString(format);
                }
            }
        }

        public void ApplyUpdate(Matrix<double> matrix)
        {
            for (int i = 0; i < matrix.RowCount; i++)
            {
                lock (_vm)
                {
                    if (i < _models.Count)
                    {
                        _models[i].RaisePropsChanged();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}