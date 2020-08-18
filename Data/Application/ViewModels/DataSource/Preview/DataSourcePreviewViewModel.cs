using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Infrastructure;
using Infrastructure.Domain;
using NNLib.Common;
using Prism.Commands;
using Prism.Regions;

namespace Data.Application.ViewModels.DataSource.Preview
{
    public class HeaderValueModel
    {
        public string Header { get; set; }
        public string Value { get; set; }
    }

    public class DataSourcePreviewViewModel : ViewModelBase<DataSourcePreviewViewModel>
    {
        private const int MaxPreviewCount = 100;

        private DataTable _fileDataPreview;
        private DataTable _dataSourceInstance;
        private HeaderValueModel _stat1;
        private HeaderValueModel _stat2;
        private int _instanceNumber;
        private DataSetInstanceAccessor _dataSetInstanceAccessor;
        private DataSetPreviewAccessor _dataSetPreviewAccessor;
        private DataSetType _instanceDataSetType;
        private DataSetType _previewDataSetType;
        private DataSetType[] _dataSetTypes = new DataSetType[0];
        private TrainingDataStats _trainingDataStats;
        private bool _showLoading;
        private int _totalInstances = 1;
        private Visibility _previewErrorVisibility = Visibility.Collapsed;
        private bool _instancePreviewLoaded;
        private bool _previewLoaded;

        //TODO
        public ICommand PreviewColumnClicked { get; } = new DelegateCommand(() => {});
        public ICommand FirstItem => new DelegateCommand(() => InstanceNumber = 1);
        public ICommand LastItem => new DelegateCommand(() => InstanceNumber = _dataSetInstanceAccessor.Count);

        internal Action Loaded;

        public DataSourcePreviewViewModel(AppState appState)
        {
            if (appState.SessionManager.ActiveSession?.TrainingData != null)
            {
                SetTrainingData(appState.SessionManager.ActiveSession.TrainingData);
            }

            if (appState.SessionManager.ActiveSession != null)
            {
                appState.SessionManager.ActiveSession.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(Session.TrainingData))
                    {
                        SetTrainingData(appState.SessionManager.ActiveSession.TrainingData);
                    }
                };
            }
            
        }

        private void SetTrainingData(TrainingData trainingData)
        {
            DataSetTypes = trainingData.SetTypes;
            DataSetInstanceAccessor = new DataSetInstanceAccessor(trainingData, DataSetType.Training);
            DataSetPreviewAccessor = new DataSetPreviewAccessor(trainingData);
            Stats = new TrainingDataStats(trainingData);

            ShowLoading = false;
        }

        internal bool PreviewLoaded
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

        internal bool InstancePreviewLoaded
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

        internal DataSetInstanceAccessor DataSetInstanceAccessor
        {
            get => _dataSetInstanceAccessor;
            set
            {
                _dataSetInstanceAccessor = value;
                InstanceNumber = 1;
                TotalInstances = _dataSetInstanceAccessor.Count;
            }
        }

        internal DataSetPreviewAccessor DataSetPreviewAccessor
        {
            get => _dataSetPreviewAccessor;
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
                _dataSetInstanceAccessor.ChangeDataSet(value);
                InstanceNumber = 1;
            }
        }

        public DataSetType PreviewDataSetType
        {
            get => _previewDataSetType;
            set
            {
                SetProperty(ref _previewDataSetType, value);
                _dataSetPreviewAccessor.ChangeDataSet(value);
                FileDataPreview = _dataSetPreviewAccessor.GetPreview(MaxPreviewCount);
                SetStats();
            }
        }

        public TrainingDataStats Stats
        {
            set
            {
                _trainingDataStats = value;
                SetStats();
            }
        }

        private void SetStats()
        {
            Stat1 = new HeaderValueModel()
            {
                Header = "Rows",
                Value = _trainingDataStats.GetRowsForSet(_previewDataSetType).ToString(),
            };
            Stat2 = new HeaderValueModel()
            {
                Header = "Columns",
                Value = _trainingDataStats.GetColumnsForSet(_previewDataSetType).ToString(),
            };
        }

        public DataTable FileDataPreview
        {
            get => _fileDataPreview;
            set => SetProperty(ref _fileDataPreview, value);
        }

        public DataTable DataSourceInstance
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
                DataSourceInstance = _dataSetInstanceAccessor[value - 1];
            }
        }

        public int TotalInstances
        {
            get => _totalInstances;
            set => SetProperty(ref _totalInstances, value);
        }

        public HeaderValueModel Stat1
        {
            get => _stat1;
            set => SetProperty(ref _stat1, value);
        }

        public HeaderValueModel Stat2
        {
            get => _stat2;
            set => SetProperty(ref _stat2, value);
        }
    }
}
