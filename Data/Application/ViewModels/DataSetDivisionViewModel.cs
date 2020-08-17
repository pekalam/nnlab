using Infrastructure;
using NNLib.Common;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Text;
using System.Windows.Data;
using Data.Application.Services;
using Data.Domain;
using Prism.Regions;

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
        private int _trainingSetPercent;
        private int _validationSetPercent;
        private int _testSetPercent;
        private bool _modifiesFileData;
        private DivisionMethod _divisionMethod;

        public DataSetDivisionViewModel(IDataSetDivisionService service)
        {
            Service = service;
            KeepAlive = false;
        }

        private void RaiseCmdCanExecChanged()
        {
            if (ModifiesFileData) Service.DivideFileDataCommand.RaiseCanExecuteChanged();
            else Service.DivideMemoryDataCommand.RaiseCanExecuteChanged();
        }

        public IDataSetDivisionService Service { get; set; }

        public DivisionMethod DivisionMethod
        {
            get => _divisionMethod;
            set => SetProperty(ref _divisionMethod, value);
        }

        public int TrainingSetPercent
        {
            get => _trainingSetPercent;
            set
            {
                SetProperty(ref _trainingSetPercent, value);
                RaiseCmdCanExecChanged();
            }
        }

        public int ValidationSetPercent
        {
            get => _validationSetPercent;
            set
            {
                SetProperty(ref _validationSetPercent, value);
                RaiseCmdCanExecChanged();
            }
        }

        public int TestSetPercent
        {
            get => _testSetPercent;
            set
            {
                SetProperty(ref _testSetPercent, value);
                RaiseCmdCanExecChanged();
            }
        }

        public bool ModifiesFileData
        {
            get => _modifiesFileData;
            set => SetProperty(ref _modifiesFileData, value);
        }


        public (List<double[]> input, List<double[]> target)? MemCmdParam { get; set; }

        public string FileCmdParam { get; set; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            ModifiesFileData = navigationContext.Parameters.ContainsKey("file") &&
                               (bool) navigationContext.Parameters["file"];

            if (ModifiesFileData)
            {
                FileCmdParam = navigationContext.Parameters["filePath"] as string;
            }
            else
            {
                MemCmdParam = (navigationContext.Parameters["input"] as List<double[]>, navigationContext.Parameters["target"] as List<double[]>);
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
                        return (TrainingSetPercent + ValidationSetPercent + TestSetPercent == 100)
                            ? null
                            : "Invalid percent value";
                    case nameof(TestSetPercent):
                        return (TrainingSetPercent + ValidationSetPercent + TestSetPercent == 100)
                            ? null
                            : "Invalid percent value";
                }

                return null;
            }
        }
    }



    public class DivisionCmdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vm = value as DataSetDivisionViewModel;

            if (vm.ModifiesFileData)
            {
                return vm.Service.DivideFileDataCommand;
            }

            return vm.Service.DivideMemoryDataCommand;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DivisionCmdParamConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var vm = value as DataSetDivisionViewModel;

            if (vm.ModifiesFileData)
            {
                return vm.FileCmdParam;
            }

            return vm.MemCmdParam;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
