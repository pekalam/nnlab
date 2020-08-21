using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Common.Framework;
using Data.Application.Services;
using Data.Domain;
using Infrastructure;
using Infrastructure.Domain;
using Prism.Regions;

namespace Data.Application.ViewModels.DataSetDivision
{
    public class FileDataSetDivisionNavParams : NavigationParameters
    {
        public FileDataSetDivisionNavParams(string filePath)
        {
            Add("file", true);
            Add(nameof(filePath), filePath);
        }
    }

    public class InMemoryDataSetDivisionNavParams : NavigationParameters
    {
        public InMemoryDataSetDivisionNavParams(List<double[]> input, List<double[]> target)
        {
            Add("file", false);
            Add(nameof(input), input);
            Add(nameof(target), target);
        }
    }

    public class DataSetDivisionViewModel : ViewModelBase<DataSetDivisionViewModel>, IDataErrorInfo
    {
        private int _trainingSetPercent;
        private int _validationSetPercent;
        private int _testSetPercent;
        private bool _modifiesFileData;
        private DivisionMethod _divisionMethod;
        private ICommand _divideCommand;
        private object _divideCommandParam;
        private string _ratio;
        private string _insufficientSizeMsg;


        public DataSetDivisionViewModel(IDataSetDivisionService service, AppState appState)
        {
            Service = service;
            KeepAlive = false;

            if (appState.SessionManager.ActiveSession?.TrainingData != null)
            {
                var data = appState.SessionManager.ActiveSession.TrainingData;
                var total = data.Sets.TrainingSet.Input.Count + (data.Sets.ValidationSet?.Input.Count ?? 0) + (data.Sets.TestSet?.Input.Count ?? 0);

                _trainingSetPercent = (int)Math.Round(data.Sets.TrainingSet.Input.Count * 100.0 / total);
                _testSetPercent = (int)Math.Round(data.Sets.TestSet?.Input.Count * 100.0 / total ?? 0);
                _validationSetPercent = (int)Math.Round(data.Sets.ValidationSet?.Input.Count * 100.0 / total ?? 0);

                UpdateRatio();
            }
        }

        public void UpdateRatio()
        {
            Ratio = $"{_trainingSetPercent}:{_validationSetPercent}:{_testSetPercent}";
        }

        private void RaiseCmdCanExecChanged()
        {
            if (ModifiesFileData) Service.DivideFileDataCommand.RaiseCanExecuteChanged();
            else Service.DivideMemoryDataCommand.RaiseCanExecuteChanged();
        }

        public IDataSetDivisionService Service { get; set; }

        public string Ratio
        {
            get => _ratio;
            set => SetProperty(ref _ratio, value);
        }

        public DivisionMethod DivisionMethod
        {
            get => _divisionMethod;
            set => SetProperty(ref _divisionMethod, value);
        }

        public string InsufficientSizeMsg
        {
            get => _insufficientSizeMsg;
            set => SetProperty(ref _insufficientSizeMsg, value);
        }

        public int TrainingSetPercent
        {
            get => _trainingSetPercent;
            set
            {
                SetProperty(ref _trainingSetPercent, value);
                RaisePropertyChanged(nameof(ValidationSetPercent));
                RaisePropertyChanged(nameof(TestSetPercent));
                RaiseCmdCanExecChanged();
            }
        }

        public int ValidationSetPercent
        {
            get => _validationSetPercent;
            set
            {
                SetProperty(ref _validationSetPercent, value);
                RaisePropertyChanged(nameof(TrainingSetPercent));
                RaisePropertyChanged(nameof(TestSetPercent));
                RaiseCmdCanExecChanged();
            }
        }

        public int TestSetPercent
        {
            get => _testSetPercent;
            set
            {
                SetProperty(ref _testSetPercent, value);
                RaisePropertyChanged(nameof(TrainingSetPercent));
                RaisePropertyChanged(nameof(ValidationSetPercent));
                RaiseCmdCanExecChanged();
            }
        }

        public bool ModifiesFileData
        {
            get => _modifiesFileData;
            set => SetProperty(ref _modifiesFileData, value);
        }

        public ICommand DivideCommand
        {
            get => _divideCommand;
            set => SetProperty(ref _divideCommand, value);
        }

        public object DivideCommandParam
        {
            get => _divideCommandParam;
            set => SetProperty(ref _divideCommandParam, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            ModifiesFileData = navigationContext.Parameters.ContainsKey("file") &&
                               (bool) navigationContext.Parameters["file"];

            if (ModifiesFileData)
            {
                DivideCommandParam = navigationContext.Parameters["filePath"] as string;
                DivideCommand = Service.DivideFileDataCommand;
            }
            else
            {
                DivideCommandParam = (navigationContext.Parameters["input"] as List<double[]>, navigationContext.Parameters["target"] as List<double[]>);
                DivideCommand = Service.DivideMemoryDataCommand;
            }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(TrainingSetPercent):
                        if (TrainingSetPercent == 0) return "Cannot set to 0";
                        return (TrainingSetPercent + ValidationSetPercent + TestSetPercent == 100)
                            ? null
                            : "Invalid percent value";
                    case nameof(ValidationSetPercent):
                    case nameof(TestSetPercent):
                        return (TrainingSetPercent + ValidationSetPercent + TestSetPercent == 100)
                            ? null
                            : "Invalid percent value";
                }

                return null;
            }
        }
    }
}
