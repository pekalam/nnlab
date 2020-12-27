using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels.DataSourcePreview;
using NNLib.Data;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Automation.Peers;
using MathNet.Numerics.LinearAlgebra;
using Unity;

namespace Data.Application.ViewModels
{
    public class HeaderValueModel
    {
        public string? Header { get; set; }
        public string? Value { get; set; }
    }

    public class FeatureStatistics
    {
        public string Variable { get; set; } = null!;
        public double Mean { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
    }

    public class TrainingDataStats
    {
        private readonly TrainingData _trainingData;

        public TrainingDataStats(TrainingData trainingData)
        {
            _trainingData = trainingData;
        }

        private static IEnumerable<Matrix<double>> Enumerate(IEnumerator<Matrix<double>> mat)
        {
            while (mat.MoveNext())
            {
                yield return mat.Current;
            }
        }

        public int GetRowsForSet(DataSetType setType)
        {
            if (setType == DataSetType.Training)
            {
                return _trainingData.Sets.TrainingSet.Input.Count;
            }

            if (setType == DataSetType.Test)
            {
                return _trainingData.Sets.TestSet!.Input.Count;
            }

            if (setType == DataSetType.Validation)
            {
                return _trainingData.Sets.ValidationSet!.Input.Count;
            }

            throw new ArgumentException();
        }

        public int GetColumnsForSet()
        {
            return _trainingData.Variables.Names.Length;
        }

        public int GetTotalRows()
        {
            return _trainingData.Sets.TrainingSet.Input.Count + (_trainingData.Sets.ValidationSet?.Input.Count ?? 0) +
                   (_trainingData.Sets.TestSet?.Input.Count ?? 0);
        }

        public FeatureStatistics[] GetFeatureStatistics()
        {
            var stats = _trainingData.Variables.Indexes.InputVarIndexes
                .Union(_trainingData.Variables.Indexes.TargetVarIndexes)
                .SelectMany(v => Enumerable.Range(0, 3).Select(i =>
            {
                SupervisedTrainingSamples? GetSet() => i switch
                {
                    0 => _trainingData.Sets.TrainingSet,
                    1 => _trainingData.Sets.ValidationSet ?? null,
                    2 => _trainingData.Sets.TestSet ?? null,
                };

                string GetSetName() => i switch
                {
                    0 => "training",
                    1 => "validation",
                    2 => "test",
                };

                IVectorSet GetVectorSet()
                {
                    var set = GetSet();
                    return _trainingData.Variables.Indexes.InputVarIndexes.Contains(v) ? set.Input : set.Target;
                }

                int GetIndex()
                {
                    if (_trainingData.Variables.Indexes.InputVarIndexes.Contains(v))
                    {
                        return _trainingData.Variables.Indexes.InputVarIndexes.IndexOf(v);
                    }
                    return _trainingData.Variables.Indexes.TargetVarIndexes.IndexOf(v);
                }

                return GetSet() != null
                    ? new FeatureStatistics()
                    {
                        Variable = $"{_trainingData.Variables.Names[v]} ({GetSetName()})",
                        Max = Enumerate(GetVectorSet().GetEnumerator()).Max(mat => mat.At(GetIndex(),0)),
                        Min = Enumerate(GetVectorSet().GetEnumerator()).Min(mat => mat.At(GetIndex(),0)),
                        Mean = Enumerate(GetVectorSet().GetEnumerator()).Average(mat => mat.At(GetIndex(),0)),
                    }
                    : null;
            })).Where(s => s != null);
            return stats.ToArray()!;
        }
    }

    public class DataSourcePreviewViewModel : ViewModelBase<DataSourcePreviewViewModel>
    {
        private const int MaxPreviewCount = 100;

        private DataTable? _fileDataPreview;
        private DataTable? _dataSourceInstance;
        private HeaderValueModel? _stat1;
        private HeaderValueModel? _stat2;
        private int _instanceNumber;
        private DataSetInstanceAccessor? _dataSetInstanceAccessor;
        private DataSetPreviewAccessor? _dataSetPreviewAccessor;
        private DataSetType _instanceDataSetType;
        private DataSetType _previewDataSetType;
        private DataSetType[] _dataSetTypes = new DataSetType[0];
        private TrainingDataStats? _trainingDataStats;
        private bool _showLoading;
        private int _totalInstances = 1;
        private bool _instancePreviewLoaded;
        private bool _previewLoaded;

        public ICommand FirstItem => new DelegateCommand(() => InstanceNumber = 1);
        public ICommand LastItem => new DelegateCommand(() => InstanceNumber = _dataSetInstanceAccessor?.Count ?? throw new NullReferenceException("Null data instance accessor"));

        public Action? Loaded;

        private readonly AppState _appState;
        private readonly AppStateHelper _helper;
        private FeatureStatistics[] _statistics;
        private HeaderValueModel? _stat3;

        public DataSourcePreviewViewModel(){}

        [InjectionConstructor]
        public DataSourcePreviewViewModel(AppState appState)
        {
            _appState = appState;
            _helper = new AppStateHelper(appState);

            _helper.OnTrainingDataChanged(SetTrainingData);
            
            _helper.OnTrainingDataPropertyChanged(data =>
            {
                if (data.Sets.ValidationSet == null && _previewDataSetType == DataSetType.Validation ||
                    data.Sets.TestSet == null && _previewDataSetType == DataSetType.Test)
                {
                    _previewDataSetType = DataSetType.Training;
                    RaisePropertyChanged(nameof(PreviewDataSetType));
                }

                DataSetTypes = data.SetTypes;
                DataSetInstanceAccessor = new DataSetInstanceAccessor(_appState, DataSetType.Training);
                DataSetPreviewAccessor = new DataSetPreviewAccessor(_appState);
                UpdateStats(new TrainingDataStats(data));
            }, s => s switch
            {
                nameof(TrainingData.Variables) => true,
                nameof(TrainingData.Sets) => true,
                nameof(TrainingData.NormalizationMethod) => true,
                _ => false,
            });
        }

        private void SetTrainingData(TrainingData trainingData)
        {
            DataSetTypes = trainingData.SetTypes;
            DataSetInstanceAccessor = new DataSetInstanceAccessor(_appState, DataSetType.Training);
            DataSetPreviewAccessor = new DataSetPreviewAccessor(_appState);
            _instanceDataSetType = DataSetType.Training;
            _previewDataSetType = DataSetType.Training;
            RaisePropertyChanged(nameof(InstanceDataSetType));
            RaisePropertyChanged(nameof(PreviewDataSetType));
            UpdateStats(new TrainingDataStats(trainingData));

            ShowLoading = false;
        }

        public bool PreviewLoaded
        {
            get => _previewLoaded;
            set
            {
                _previewLoaded = value;
                if (InstancePreviewLoaded)
                {
                    Loaded?.Invoke();
                }
            }
        }

        public bool InstancePreviewLoaded
        {
            get => _instancePreviewLoaded;
            set
            {
                _instancePreviewLoaded = value;
                if (PreviewLoaded)
                {
                    Loaded?.Invoke();
                }
            }
        }

        public DataSetInstanceAccessor? DataSetInstanceAccessor
        {
            get => _dataSetInstanceAccessor;
            set
            {
                _dataSetInstanceAccessor = value!;
                InstanceNumber = 1;
                TotalInstances = _dataSetInstanceAccessor.Count;
            }
        }

        public DataSetPreviewAccessor DataSetPreviewAccessor
        {
            get => _dataSetPreviewAccessor!;
            set
            {
                _dataSetPreviewAccessor = value;
                FileDataPreview = _dataSetPreviewAccessor.GetPreview(MaxPreviewCount);
            }
        }

        public bool ShowLoading
        {
            get => _showLoading;
            set => SetProperty(ref _showLoading, value);
        }

        public DataSetType[] DataSetTypes
        {
            get => _dataSetTypes;
            set => SetProperty(ref _dataSetTypes, value);
        }

        public DataSetType InstanceDataSetType
        {
            get => _instanceDataSetType;
            set
            {
                SetProperty(ref _instanceDataSetType, value);
                _dataSetInstanceAccessor!.ChangeDataSet(value);
                InstanceNumber = 1;
            }
        }

        public DataSetType PreviewDataSetType
        {
            get => _previewDataSetType;
            set
            {
                SetProperty(ref _previewDataSetType, value);
                _dataSetPreviewAccessor!.ChangeDataSet(value);
                FileDataPreview = _dataSetPreviewAccessor.GetPreview(MaxPreviewCount);
                UpdateStats();
            }
        }

        private void UpdateStats(TrainingDataStats? newStats = null)
        {
            if (newStats != null)
            {
                _trainingDataStats = newStats;
            }

            Stat1 = new HeaderValueModel()
            {
                Header = "Rows",
                Value = _trainingDataStats!.GetRowsForSet(_previewDataSetType).ToString(),
            };
            Stat2 = new HeaderValueModel()
            {
                Header = "Columns",
                Value = _trainingDataStats.GetColumnsForSet().ToString(),
            };
            Stat3 = new HeaderValueModel()
            {
                Header = "Total rows",
                Value = _trainingDataStats.GetTotalRows().ToString(),
            };
            Statistics = _trainingDataStats.GetFeatureStatistics();
        }

        public DataTable? FileDataPreview
        {
            get => _fileDataPreview;
            set => SetProperty(ref _fileDataPreview, value);
        }

        public DataTable? DataSourceInstance
        {
            get => _dataSourceInstance;
            set => SetProperty(ref _dataSourceInstance, value);
        }

        public int InstanceNumber
        {
            get => _instanceNumber;
            set
            {
                SetProperty(ref _instanceNumber, value);
                DataSourceInstance = _dataSetInstanceAccessor![value - 1];
            }
        }

        public int TotalInstances
        {
            get => _totalInstances;
            set => SetProperty(ref _totalInstances, value);
        }

        public HeaderValueModel? Stat1
        {
            get => _stat1;
            set => SetProperty(ref _stat1, value);
        }

        public HeaderValueModel? Stat2
        {
            get => _stat2;
            set => SetProperty(ref _stat2, value);
        }

        public HeaderValueModel? Stat3
        {
            get => _stat3;
            set => SetProperty(ref _stat3, value);
        }

        public FeatureStatistics[] Statistics
        {
            get => _statistics;
            set => SetProperty(ref _statistics, value);
        }
    }
}
