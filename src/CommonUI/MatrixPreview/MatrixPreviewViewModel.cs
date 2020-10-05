using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CommonServiceLocator;
using ControlzEx.Standard;
using MathNet.Numerics.LinearAlgebra;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace SharedUI.MatrixPreview
{
    public class MatrixPreviewViewModel : BindableBase
    {
        private int _selectedLayerNum;
        private MatrixTypes _selectedMatrixType;
        private int _numPrecision = 2;
        private List<MatrixPreviewModel>? _source;
        private bool _readOnly = true;
        private bool _canRemoveItem;

        [InjectionConstructor]
        public MatrixPreviewViewModel(IEventAggregator ea)
        {
            Controller = new MatrixPreviewController(this,ea);
            NextLayer = Controller.NextLayer;
            PrevLayer = Controller.PrevLayer;
            IncreasePrecision = Controller.IncreasePrecision;
            DecreasePrecision = Controller.DecreasePrecision;
            ColumnClicked = Controller.ColumnClicked;
        }

        public MatrixPreviewViewModel() : this(ServiceLocator.Current.GetInstance<IEventAggregator>())
        {
        }

        public ICommand NextLayer { get; }
        public ICommand PrevLayer { get; }
        public ICommand IncreasePrecision { get; }
        public ICommand DecreasePrecision { get; }
        public ICommand ColumnClicked { get; }

        internal Action<IEnumerable<DataGridColumn>>? UpdateColumns { get; set; }
        public event Action? GridInitialized;
        public event Action<Matrix<double>>? RowRemoved;
        public event Action<Matrix<double>>? MatrixElementChanged; 
        internal void RaiseGridInitialized() => GridInitialized?.Invoke();
        internal void RaiseRowRemoved(Matrix<double> newMatrix) => RowRemoved?.Invoke(newMatrix);
        internal void RaiseMatrixElementChanged(Matrix<double> mat) => MatrixElementChanged?.Invoke(mat);


        public MatrixPreviewController Controller { get; }
        public int SelectedLayerNum
        {
            get => _selectedLayerNum;
            set => SetProperty(ref _selectedLayerNum, value);
        }

        public bool ReadOnly
        {
            get => _readOnly;
            set => SetProperty(ref _readOnly, value);
        }

        public List<MatrixPreviewModel>? Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public MatrixTypes SelectedMatrixType
        {
            get => _selectedMatrixType;
            set => SetProperty(ref _selectedMatrixType, value);
        }

        public int NumPrecision
        {
            get => _numPrecision;
            set => SetProperty(ref _numPrecision, value);
        }

        public bool CanRemoveItem
        {
            get => _canRemoveItem;
            set => SetProperty(ref _canRemoveItem, value);
        }
    }
}