using Data.Application.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Infrastructure.Domain;
using NNLib.Common;
using Prism.Commands;

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
            var method = new LinearDataSetDivider();

            var pos = args.Value.input.Select((doubles, i) => (long)i).ToList();

            var divs = method.Divide(pos, ConstructDivOptions());

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

            _appState.SessionManager.ActiveSession.TrainingData = new TrainingData(sets, existing.Variables, TrainingDataSource.Memory);
        }

        private void DivideFileData(string path)
        {
            var existingData = _appState.SessionManager.ActiveSession.TrainingData;
            _appState.SessionManager.ActiveSession.TrainingData = _dataSetService.LoadSets(path, new LinearDataSetDivider(), ConstructDivOptions(), existingData.Variables.Indexes);
        }
    }
}
