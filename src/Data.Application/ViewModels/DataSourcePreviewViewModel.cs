using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels.DataSourcePreview;
using NNLib.Data;
using Prism.Commands;
using System;
using System.Data;
using System.Windows;
using System.Windows.Input;

namespace Data.Application.ViewModels
{
    public class HeaderValueModel
    {
        public string? Header { get; set; }
        public string? Value { get; set; }
    }

    public class TrainingDataStats
    {
        private readonly TrainingData _trainingData;

        public TrainingDataStats(TrainingData trainingData)
        {
            _trainingData = trainingData;
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

        public int GetColumnsForSet(DataSetType setType)
        {
            return _trainingData.Variables.Names.Length;
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
        private Visibility _previewErrorVisibility = Visibility.Collapsed;
        private bool _instancePreviewLoaded;
        private bool _previewLoaded;

        //TODO
        public ICommand PreviewColumnClicked { get; } = new DelegateCommand(() => {});
        public ICommand FirstItem => new DelegateCommand(() => InstanceNumber = 1);
        public ICommand LastItem => new DelegateCommand(() => InstanceNumber = _dataSetInstanceAccessor?.Count ?? throw new NullReferenceException("Null data instance accessor"));

        public Action? Loaded;

        private readonly AppState _appState;
        private readonly AppStateHelper _helper;

        public DataSourcePreviewViewModel(AppState appState)
        {
            _appState = appState;
            _helper = new AppStateHelper(appState);

            _helper.OnTrainingDataChanged(SetTrainingData);
            
            _helper.OnTrainingDataPropertyChanged(data =>
            {
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

        public Visibility PreviewErrorVisibility
        {
            get => _previewErrorVisibility;
            set => SetProperty(ref _previewErrorVisibility, value);
        }

        public void ShowPreviewError(bool show)
        {
            if (show)
            {
                PreviewErrorVisibility = Visibility.Visible;
            }
            else
            {
                PreviewErrorVisibility = Visibility.Collapsed;
            }
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
                Value = _trainingDataStats.GetColumnsForSet(_previewDataSetType).ToString(),
            };
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
    }
}
