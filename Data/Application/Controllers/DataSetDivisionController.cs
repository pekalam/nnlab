using Data.Application.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Data.Application.ViewModels;
using Data.Application.ViewModels.DataSetDivision;
using Data.Domain.Services;
using Data.Presentation.Views.DataSetDivision;
using Infrastructure;
using Infrastructure.Domain;
using Infrastructure.Messaging;
using NNLib.Common;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Data.Application.Controllers
{


    internal class DataSetDivisionController
    {
        private readonly DataSetDivisionService _service;
        private readonly AppState _appState;
        private readonly ISupervisedDataSetService _dataSetService;

        public DataSetDivisionController(DataSetDivisionService service, ISupervisedDataSetService dataSetService, AppState appState)
        {
            _service = service;
            _dataSetService = dataSetService;
            _appState = appState;

            _service.DivideFileDataCommand =
                new DelegateCommand<string>(DivideFileData, _ => CanDivide());

            _service.DivideMemoryDataCommand = new DelegateCommand<(List<double[]> input, List<double[]> target)?>(DivideMemoryData, _ => CanDivide());

            DataSetDivisionViewModel.Created += () =>
            {
                var vm = DataSetDivisionViewModel.Instance;
                vm.PropertyChanged += VmOnPropertyChanged;
            };
        }

        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(DataSetDivisionViewModel.TrainingSetPercent):
                case nameof(DataSetDivisionViewModel.ValidationSetPercent):
                case nameof(DataSetDivisionViewModel.TestSetPercent):
                    CalcHasSufficientSize();
                    break;

            }
        }

        private void CalcHasSufficientSize()
        {
            var vm = DataSetDivisionViewModel.Instance;
            if (!CanDivide()) return;

            if (_appState.SessionManager.ActiveSession?.TrainingData != null)
            {
                var data = _appState.SessionManager.ActiveSession.TrainingData;
                var total = data.Sets.TrainingSet.Input.Count + (data.Sets.ValidationSet?.Input.Count ?? 0) + (data.Sets.TestSet?.Input.Count ?? 0);
                var left = total;

                var trainingSetCount = (int)Math.Ceiling(vm.TrainingSetPercent * total / 100f);
                trainingSetCount = trainingSetCount > left ? left : trainingSetCount;
                left -= trainingSetCount;

                var validationSetCount = (int)Math.Ceiling(vm.ValidationSetPercent * total / 100f);
                validationSetCount = validationSetCount > left ? left : validationSetCount;
                left -= validationSetCount;

                var testSetCount = (int)Math.Ceiling(vm.TestSetPercent * total / 100f);
                testSetCount = testSetCount > left ? left : testSetCount;

                var calcTrainingPerc = (int)Math.Round(trainingSetCount * 100.0 / total);
                var calcValidationPerc = (int)Math.Round(validationSetCount * 100.0 / total);
                var calcTestPerc = (int)Math.Round(testSetCount * 100.0 / total);


                if (vm.TrainingSetPercent != calcTrainingPerc || vm.ValidationSetPercent != calcValidationPerc ||
                    vm.TestSetPercent != calcTestPerc)
                {
                    vm.InsufficientSizeMsg = $"Data set will be divided by ratio: {calcTrainingPerc}:{calcValidationPerc}:{calcTestPerc}";
                }
                else
                {
                    vm.InsufficientSizeMsg = default;
                }

            }

        }

        private bool CanDivide()
        {
            var vm = DataSetDivisionViewModel.Instance;
            return vm.TrainingSetPercent > 0 && vm.TrainingSetPercent + vm.ValidationSetPercent + vm.TestSetPercent == 100;
        }

        private DataSetDivisionOptions ConstructDivOptions()
        {
            var vm = DataSetDivisionViewModel.Instance;
            return new DataSetDivisionOptions()
            {
                TrainingSetPercent = vm.TrainingSetPercent,
                ValidationSetPercent = vm.ValidationSetPercent,
                TestSetPercent = vm.TestSetPercent,
            };
        }

        private void DivideMemoryData((List<double[]> input, List<double[]> target)? args)
        {
            var vm = DataSetDivisionViewModel.Instance;
            var method = new LinearDataSetDivider();

            var pos = args.Value.input.Select((doubles, i) => (long)i).ToList();

            var opt = ConstructDivOptions();
            var divs = method.Divide(pos, opt);

            SupervisedSet? training = null;
            SupervisedSet? test = null;
            SupervisedSet? validation = null;

            foreach (var div in divs)
            {
                var input = args.Value.input.Where((doubles, i) => div.positions.Contains(i)).ToArray();
                var target = args.Value.target.Where((doubles, i) => div.positions.Contains(i)).ToArray();

                if (div.setType == DataSetType.Training)
                {
                    training = SupervisedSet.FromArrays(input, target);
                }

                if (div.setType == DataSetType.Test)
                {
                    test = SupervisedSet.FromArrays(input, target);
                }

                if (div.setType == DataSetType.Validation)
                {
                    validation = SupervisedSet.FromArrays(input, target);
                }
            }

            if (training == null) throw new Exception("Training set not present after data set division");

            var sets = new SupervisedTrainingSets(training)
            {
                TestSet = test,
                ValidationSet = validation
            };

            var existing = _appState.SessionManager.ActiveSession.TrainingData;


            var total = sets.TrainingSet.Input.Count + (sets.ValidationSet?.Input.Count ?? 0) +
                        (sets.TestSet?.Input.Count ?? 0);
            var calcTrainingPerc = (int)Math.Round(sets.TrainingSet.Input.Count * 100.0 / total);
            var calcValidationPerc = (int)Math.Round((sets.ValidationSet?.Input.Count ?? 0) * 100.0 / total);
            var calcTestPerc = (int)Math.Round((sets.TestSet?.Input.Count ?? 0) * 100.0 / total);

            vm.PropertyChanged -= VmOnPropertyChanged;
            vm.TrainingSetPercent = calcTrainingPerc;
            vm.ValidationSetPercent = calcValidationPerc;
            vm.TestSetPercent = calcTestPerc;
            vm.InsufficientSizeMsg = default;
            vm.UpdateRatio();
            vm.PropertyChanged += VmOnPropertyChanged;

            _appState.SessionManager.ActiveSession.TrainingData = new TrainingData(sets, existing.Variables, TrainingDataSource.Memory);
        }

        private void DivideFileData(string path)
        {
            var existingData = _appState.SessionManager.ActiveSession.TrainingData;
            _appState.SessionManager.ActiveSession.TrainingData = _dataSetService.LoadSets(path, new LinearDataSetDivider(), ConstructDivOptions(), existingData.Variables.Indexes);
        }
    }
}
