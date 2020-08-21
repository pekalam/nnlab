using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using Prism.Commands;
using Prism.Events;

namespace CommonUI.MatrixPreview
{
    public class MatrixPreviewController
    {
        public const int MAX_PRECISION = 15;
        public const int MIN_PRECISION = 2;

        private readonly IEventAggregator _ea;
        private readonly MatrixPreviewViewModel _vm;
        private MLPNetwork? _network;
        private Matrix<double>? _assignedMatrix;

        private MatrixTypes _selectedType;
        private int _selectedLayerNum;
        private volatile bool _disableUpdate;
        private string _numFormat = "F2";

        //todo clear
        private readonly Dictionary<int, MatrixTypes> _cachedSelection = new Dictionary<int, MatrixTypes>();
        private readonly MatrixGridRenderer _matrixGridRenderer;

        public MatrixPreviewController(MatrixPreviewViewModel vm, IEventAggregator ea)
        {
            _ea = ea;
            _vm = vm;
            _matrixGridRenderer = new MatrixGridRenderer(_vm);

            NextLayer = new DelegateCommand(NextLayerExecute);
            PrevLayer = new DelegateCommand(PrevLayerExecute);
            IncreasePrecision = new DelegateCommand(() =>
            {
                if (_vm.NumPrecision < MAX_PRECISION)
                {
                    _vm.NumPrecision++;
                    _numFormat = "F" + _vm.NumPrecision;
                    UpdatePreview();
                    ApplyUpdate();
                }
            });
            DecreasePrecision = new DelegateCommand(() =>
            {
                if (_vm.NumPrecision > MIN_PRECISION)
                {
                    _vm.NumPrecision--;
                    _numFormat = "F" + _vm.NumPrecision;

                    UpdatePreview();
                    ApplyUpdate();
                }
            });

            ColumnClicked = new DelegateCommand<int?>((columnIndex) =>
            {
                //_ea.GetEvent<MatrixPreviewColumnClicked>().Publish((_selectedLayerNum, columnIndex.Value));
            });

            _vm.PropertyChanged += VmOnPropertyChanged;
        }


        public ICommand NextLayer { get; }
        public ICommand PrevLayer { get; }
        public ICommand IncreasePrecision { get; }
        public ICommand DecreasePrecision { get; }
        public ICommand ColumnClicked { get; }
        public Matrix<double>? AssignedMatrix => _assignedMatrix;

        public void Update()
        {
            if (_disableUpdate) return;
            lock (_vm)
            {
                UpdatePreview();
            }
        }

        public void ApplyUpdate()
        {
            lock (_vm)
            {
                var matrix = GetSelectedMatrix();
                _matrixGridRenderer.ApplyUpdate(matrix);
            }
        }

        public void AssignNetwork(MLPNetwork network)
        {
            _network = network;
            CreateGrid();
        }

        public void AssignMatrix(Matrix<double> mat)
        {
            _assignedMatrix = mat;
            CreateGrid();
        }

        public void SelectMatrix(int layerNum, MatrixTypes type)
        {
            _vm.SelectedLayerNum = layerNum;
            _vm.SelectedMatrixType = type;
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MatrixPreviewViewModel.SelectedMatrixType))
            {
                _cachedSelection[_vm.SelectedLayerNum] = _vm.SelectedMatrixType;

                lock (_vm)
                {
                    _disableUpdate = true;
                    _selectedLayerNum = _vm.SelectedLayerNum;
                    _selectedType = _vm.SelectedMatrixType;
                    CreateGrid();
                    UpdatePreview();
                    ApplyUpdate();
                }

                _disableUpdate = false;
            }

            if (e.PropertyName == nameof(MatrixPreviewViewModel.SelectedLayerNum))
            {
                if (_cachedSelection.ContainsKey(_vm.SelectedLayerNum))
                {
                    _vm.SelectedMatrixType = _cachedSelection[_vm.SelectedLayerNum];
                    if (_selectedType != _vm.SelectedMatrixType)
                    {
                        return;
                    }

                    _selectedType = _vm.SelectedMatrixType;
                }

                lock (_vm)
                {
                    _disableUpdate = true;
                    _selectedLayerNum = _vm.SelectedLayerNum;
                    CreateGrid();
                    UpdatePreview();
                    ApplyUpdate();
                }

                _disableUpdate = false;
            }

            if (e.PropertyName == nameof(MatrixPreviewViewModel.NumPrecision))
            {
                if (_vm.NumPrecision != 0)
                {
                    _numFormat = "F" + _vm.NumPrecision;
                }
                else
                {
                    _numFormat = "F";
                }

                UpdatePreview();
                ApplyUpdate();
            }

            if (e.PropertyName == nameof(MatrixPreviewViewModel.ReadOnly))
            {
                _matrixGridRenderer.ReadOnly = _vm.ReadOnly;
                if (_matrixGridRenderer.Rendered)
                {
                    CreateGrid();
                }
            }
        }

        private void NextLayerExecute()
        {
            Debug.Assert(_network != null, nameof(_network) + " != null");

            _cachedSelection[_vm.SelectedLayerNum] = _vm.SelectedMatrixType;

            if (_vm.SelectedLayerNum + 1 >= _network.TotalLayers)
            {
                _vm.SelectedLayerNum = _network.TotalLayers - 1;
            }
            else
            {
                _vm.SelectedLayerNum++;
            }
        }

        private void PrevLayerExecute()
        {
            _cachedSelection[_vm.SelectedLayerNum] = _vm.SelectedMatrixType;

            if (_vm.SelectedLayerNum - 1 < 0)
            {
                _vm.SelectedLayerNum = 0;
            }
            else
            {
                _vm.SelectedLayerNum--;
            }
        }

        private Matrix<double> GetSelectedMatrix()
        {
            if (_network == null)
            {
                return _assignedMatrix;
            }
            
            if (_selectedType == MatrixTypes.Biases)
            {
                return _network.Layers[_selectedLayerNum].Biases;
            }

            if (_selectedType == MatrixTypes.Output)
            {
                return _network.Layers[_selectedLayerNum].Output;
            }

            if (_selectedType == MatrixTypes.Weights)
            {
                return _network.Layers[_selectedLayerNum].Weights;
            }

            throw new Exception("Unknown matrix type");
        }

        private void CreateGrid()
        {
            string columnTitle = string.Empty;
            switch (_selectedType)
            {
                case MatrixTypes.Biases:
                    columnTitle = "Bias";
                    break;
                case MatrixTypes.Weights:
                    columnTitle = "Weight";
                    break;
                case MatrixTypes.Output:
                    columnTitle = "Output";
                    break;
            }

            var matrix = GetSelectedMatrix(); 
            _matrixGridRenderer.Create(matrix, _numFormat, i => columnTitle + " " + i, i => "Neuron " + i);
            
        }

        private void UpdatePreview()
        {
            var matrix = GetSelectedMatrix();
            _matrixGridRenderer.Update(matrix, _numFormat);
        }
    }
}