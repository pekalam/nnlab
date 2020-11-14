using MathNet.Numerics.LinearAlgebra;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SharedUI.MatrixPreview
{
    internal class MatrixGridRenderer
    {
        private readonly MatrixPreviewViewModel _vm;
        private List<MatrixPreviewModel>? _models;
        private readonly List<DataGridColumn> _columns = new List<DataGridColumn>();
        private readonly DelegateCommand<MatrixPreviewModel> _removeCommand;

        public MatrixGridRenderer(MatrixPreviewViewModel vm, DelegateCommand<MatrixPreviewModel> removeCommand)
        {
            _vm = vm;
            _removeCommand = removeCommand;
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

                if (_vm.CanRemoveItem && Application.Current != null)
                {
                    var template = Application.Current.Resources["MatrixModelRemoveCellTemplate"] as DataTemplate;
                    _columns.Add(new DataGridTemplateColumn()
                    {
                        CellTemplate = template,
                    });
                }

                for (int i = 0; i < matrix.RowCount; i++)
                {
                    var model = new MatrixPreviewModel
                    {
                        RowIndex = i,
                        Props = new MatrixRowDictionary(),
                        RowHeader = rowTitle(i),
                        RemoveCommand = _removeCommand,
                    };

                    model.Props.ElementChanged += () => _vm.RaiseMatrixElementChanged(matrix);

                    for (int j = 0; j < matrix.ColumnCount; j++)
                    {
                        model.Props.UpdateCol(j, matrix.At(i, j).ToString(format));
                    }

                    if (!ReadOnly)
                    {
                        model.Props.AssignMatrix(matrix, i);
                    }

                    _models.Add(model);
                }

                _vm.UpdateColumns?.Invoke(_columns);
                _vm.Source = _models;
            }
        }

        public void Update(Matrix<double> matrix, string format)
        {
            Debug.Assert(_models != null, nameof(_models) + " != null");

            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    _models[i].Props.UpdateCol(j, matrix.At(i, j).ToString(format));
                }
            }
        }

        public void ApplyUpdate(Matrix<double> matrix)
        {
            Debug.Assert(_models != null, nameof(_models) + " != null");

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