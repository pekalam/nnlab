using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using CommonServiceLocator;
using Prism.Events;
using Prism.Mvvm;

namespace SharedUI.MatrixPreview
{
    public class MatrixPreviewViewModel : BindableBase
    {
        private int _selectedLayerNum;
        private MatrixTypes _selectedMatrixType;
        private int _numPrecision = 2;
        private List<MatrixPreviewModel> _source;
        private bool _readOnly = true;

        public MatrixPreviewViewModel()
        {
            Controller = new MatrixPreviewController(this, ServiceLocator.Current.GetInstance<IEventAggregator>());
            NextLayer = Controller.NextLayer;
            PrevLayer = Controller.PrevLayer;
            IncreasePrecision = Controller.IncreasePrecision;
            DecreasePrecision = Controller.DecreasePrecision;
            ColumnClicked = Controller.ColumnClicked;
        }

        public ICommand NextLayer { get; }
        public ICommand PrevLayer { get; }
        public ICommand IncreasePrecision { get; }
        public ICommand DecreasePrecision { get; }
        public ICommand ColumnClicked { get; }

        internal Action<IEnumerable<DataGridColumn>>? UpdateColumns { get; set; }
        public event Action GridInitialized;
        internal void RaiseGridInitialized() => GridInitialized?.Invoke();

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

        public List<MatrixPreviewModel> Source
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
    }
}