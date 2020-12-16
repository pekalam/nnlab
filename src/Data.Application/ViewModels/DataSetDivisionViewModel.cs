using Common.Domain;
using Common.Framework;
using Data.Application.Controllers;
using Data.Domain;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Data.Application.ViewModels
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
        private decimal _trainingSetPercent;
        private decimal _validationSetPercent;
        private decimal _testSetPercent;
        private bool _modifiesFileData;
        private DivisionMethod _divisionMethod;
        private ICommand _divideCommand = null!;
        private object? _divideCommandParam;
        private string? _ratio;
        private string? _insufficientSizeMsg;
        private string? _ratioModificationMsg;


        public DataSetDivisionViewModel(IDataSetDivisionController service)
        {
            Service = service;
            KeepAlive = false;

            service.Initialize(this);
        }

        public void UpdateRatio(decimal training, decimal validation, decimal test)
        {
            var t = (training % 1 == 0 ? training.ToString("F0") : Math.Round(training, 2).ToString());
            var v = (validation % 1 == 0 ? validation.ToString("F0") : Math.Round(validation, 2).ToString());
            var ts = (test % 1 == 0 ? test.ToString("F0") : Math.Round(test, 2).ToString());
            Ratio = $"{t}%:{v}%:{ts}%";
        }

        private void RaiseCmdCanExecChanged()
        {
            if (ModifiesFileData) Service.DivideFileDataCommand.RaiseCanExecuteChanged();
            else Service.DivideMemoryDataCommand.RaiseCanExecuteChanged();
        }

        public IDataSetDivisionController Service { get; set; }

        public string? Ratio
        {
            get => _ratio;
            set => SetProperty(ref _ratio, value);
        }

        public DivisionMethod DivisionMethod
        {
            get => _divisionMethod;
            set => SetProperty(ref _divisionMethod, value);
        }

        public string? InsufficientSizeMsg
        {
            get => _insufficientSizeMsg;
            set => SetProperty(ref _insufficientSizeMsg, value);
        }

        public string? RatioModificationMsg
        {
            get => _ratioModificationMsg;
            set => SetProperty(ref _ratioModificationMsg, value);
        }

        public decimal TrainingSetPercent
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

        public decimal ValidationSetPercent
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

        public decimal TestSetPercent
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
            get => _divideCommandParam!;
            set => SetProperty(ref _divideCommandParam, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            ModifiesFileData = navigationContext.Parameters.ContainsKey("file") &&
                               (bool) navigationContext.Parameters["file"];

            if (ModifiesFileData)
            {
                DivideCommandParam = (string)navigationContext.Parameters["filePath"];
                DivideCommand = Service.DivideFileDataCommand;
            }
            else
            {
                DivideCommandParam = (navigationContext.Parameters["input"] as List<double[]>, navigationContext.Parameters["target"] as List<double[]>);
                DivideCommand = Service.DivideMemoryDataCommand;
            }
            Debug.Assert(DivideCommand != null);
        }

        public string? Error => null;

        public string? this[string columnName]
        {
            get
            {
                bool IsDefaultPercents()
                {
                    return TrainingSetPercent == 0 && ValidationSetPercent == 0 && TestSetPercent == 0;
                }
                switch (columnName)
                {
                    case nameof(TrainingSetPercent):
                        if (TrainingSetPercent == 0 && (ValidationSetPercent > 0 || TestSetPercent > 0)) return "Cannot set to 0";
                        return Service.ValidPercents() || IsDefaultPercents() ? null : "Invalid percent value";
                    case nameof(ValidationSetPercent):
                    case nameof(TestSetPercent):
                        return Service.ValidPercents() || IsDefaultPercents() ? null : "Invalid percent value";
                }

                return null;
            }
        }
    }
}
