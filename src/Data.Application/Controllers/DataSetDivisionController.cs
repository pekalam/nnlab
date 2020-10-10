using Data.Application.Services;
using Data.Application.ViewModels.DataSetDivision;
using Data.Domain.Services;
using NNLib.Common;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Common.Domain;
using Common.Framework;
using Prism.Ioc;

namespace Data.Application.Controllers
{
    internal interface IDataSetDivisionController : ISingletonController
    {
        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IDataSetDivisionController, DataSetDivisionController>();
        }
    }

    internal class DataSetDivisionController : ControllerBase<DataSetDivisionViewModel>,IDataSetDivisionController
    {
        private readonly AppState _appState;
        private readonly ITrainingDataService _dataService;
        private readonly DataSetDivisionService _service;

        public DataSetDivisionController(DataSetDivisionService service, ITrainingDataService dataService, AppState appState, IViewModelAccessor accessor) : base(accessor)
        {
            _service = service;
            _dataService = dataService;
            _appState = appState;

            service.DivideFileDataCommand =
                new DelegateCommand<string>(DivideFileData, _ => CanDivide());

            service.DivideMemoryDataCommand = new DelegateCommand<(List<double[]> input, List<double[]> target)?>(DivideMemoryData, _ => CanDivide());
        }

        protected override void VmCreated()
        {
            Vm!.PropertyChanged += VmOnPropertyChanged;
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
            _service.DivideFileDataCommand.RaiseCanExecuteChanged();
            _service.DivideMemoryDataCommand.RaiseCanExecuteChanged();
            if (!CanDivide()) return;

            if (_appState.ActiveSession?.TrainingData != null)
            {
                var data = _appState.ActiveSession.TrainingData;
                var total = data.Sets.TrainingSet.Input.Count + (data.Sets.ValidationSet?.Input.Count ?? 0) + (data.Sets.TestSet?.Input.Count ?? 0);
                var left = total;

                var trainingSetCount = (int)Math.Ceiling(Vm!.TrainingSetPercent * total / 100f);
                trainingSetCount = trainingSetCount > left ? left : trainingSetCount;
                left -= trainingSetCount;

                var validationSetCount = (int)Math.Ceiling(Vm!.ValidationSetPercent * total / 100f);
                validationSetCount = validationSetCount > left ? left : validationSetCount;
                left -= validationSetCount;

                var testSetCount = (int)Math.Ceiling(Vm!.TestSetPercent * total / 100f);
                testSetCount = testSetCount > left ? left : testSetCount;

                var calcTrainingPerc = (int)Math.Round(trainingSetCount * 100.0 / total);
                var calcValidationPerc = (int)Math.Round(validationSetCount * 100.0 / total);
                var calcTestPerc = (int)Math.Round(testSetCount * 100.0 / total);


                if (Vm!.TrainingSetPercent != calcTrainingPerc || Vm!.ValidationSetPercent != calcValidationPerc ||
                    Vm!.TestSetPercent != calcTestPerc)
                {
                    Vm!.InsufficientSizeMsg = $"Data set will be divided by ratio: {calcTrainingPerc}:{calcValidationPerc}:{calcTestPerc}";
                }
                else
                {
                    Vm!.InsufficientSizeMsg = default;
                }

            }

        }

        private bool CanDivide()
        {
            return Vm!.TrainingSetPercent > 0 && Vm!.TrainingSetPercent + Vm!.ValidationSetPercent + Vm!.TestSetPercent == 100;
        }

        private DataSetDivisionOptions ConstructDivOptions()
        {
            return new DataSetDivisionOptions()
            {
                TrainingSetPercent = Vm!.TrainingSetPercent,
                ValidationSetPercent = Vm!.ValidationSetPercent,
                TestSetPercent = Vm!.TestSetPercent,
            };
        }

        private void DivideMemoryData((List<double[]> input, List<double[]> target)? args)
        {
            var method = new LinearDataSetDivider();

            var pos = args!.Value!.input.Select((doubles, i) => (long)i).ToList();

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

            var total = sets.TrainingSet.Input.Count + (sets.ValidationSet?.Input.Count ?? 0) +
                        (sets.TestSet?.Input.Count ?? 0);
            var calcTrainingPerc = (int)Math.Round(sets.TrainingSet.Input.Count * 100.0 / total);
            var calcValidationPerc = (int)Math.Round((sets.ValidationSet?.Input.Count ?? 0) * 100.0 / total);
            var calcTestPerc = (int)Math.Round((sets.TestSet?.Input.Count ?? 0) * 100.0 / total);

            Vm!.PropertyChanged -= VmOnPropertyChanged;
            Vm!.TrainingSetPercent = calcTrainingPerc;
            Vm!.ValidationSetPercent = calcValidationPerc;
            Vm!.TestSetPercent = calcTestPerc;
            Vm!.InsufficientSizeMsg = default;
            Vm!.UpdateRatio();
            Vm!.PropertyChanged += VmOnPropertyChanged;

            _appState.ActiveSession!.TrainingData!.Sets = sets;
            _appState.ActiveSession.RaiseTrainingDataUpdated();
        }

        private void DivideFileData(string path)
        {
            var existingData = _appState.ActiveSession!.TrainingData!;
            var opt = ConstructDivOptions();
            _appState.ActiveSession.TrainingData!.Sets = _dataService.LoadSets(path, new LinearDataSetDivider(), opt, existingData.Variables.Indexes);
            _appState.ActiveSession.RaiseTrainingDataUpdated();

            Vm!.PropertyChanged -= VmOnPropertyChanged;
            Vm!.TrainingSetPercent = opt.TrainingSetPercent;
            Vm!.ValidationSetPercent = opt.ValidationSetPercent;
            Vm!.TestSetPercent = opt.TestSetPercent;
            Vm!.UpdateRatio();
            Vm!.PropertyChanged += VmOnPropertyChanged;
        }

        public void Initialize()
        {
            
        }
    }
}
