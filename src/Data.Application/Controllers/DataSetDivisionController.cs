using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using Data.Domain;
using Data.Domain.Services;
using NNLib.Common;
using NNLib.Data;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Data.Application.Controllers
{
    public interface IDataSetDivisionController : IController
    {
        DelegateCommand<string> DivideFileDataCommand { get; set; }
        DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }

        DelegateCommand<object> Divide13Command { get; set; }
        DelegateCommand<object> Divide7020Command { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IDataSetDivisionController, DataSetDivisionController>();
        }
    }

    internal class DataSetDivisionController : ControllerBase<DataSetDivisionViewModel>,IDataSetDivisionController
    {
        private readonly AppState _appState;
        private readonly ITrainingDataService _dataService;

        public DataSetDivisionController( ITrainingDataService dataService, AppState appState)
        {
            _dataService = dataService;
            _appState = appState;

            DivideFileDataCommand =
                new DelegateCommand<string>(s => DivideFileData(s), _ => CanDivide());

            DivideMemoryDataCommand = new DelegateCommand<(List<double[]> input, List<double[]> target)?>(args => DivideMemoryData(args), _ => CanDivide());


            Divide13Command = new DelegateCommand<object>(o =>
            {
                Vm!.InsufficientSizeMsg = default;
                var opt = new DataSetDivisionOptions()
                {
                    TrainingSetPercent = Vm!.TrainingSetPercent = 33,
                    TestSetPercent = Vm!.TestSetPercent = 33,
                    ValidationSetPercent = Vm!.ValidationSetPercent = 33,
                };

                if(Vm!.InsufficientSizeMsg != default) return;

                if (o is string path)
                {
                    DivideFileData(path, opt);
                }
                else
                {
                    DivideMemoryData(((List<double[]> input, List<double[]> target)?)o, opt);
                }
            });


            Divide7020Command = new DelegateCommand<object>(o =>
            {
                Vm!.InsufficientSizeMsg = default;
                var opt = new DataSetDivisionOptions()
                {
                    TrainingSetPercent = Vm!.TrainingSetPercent = 70,
                    ValidationSetPercent = Vm!.ValidationSetPercent = 20,
                    TestSetPercent = Vm!.TestSetPercent = 10,
                };

                if (Vm!.InsufficientSizeMsg != default) return;

                if (o is string path)
                {
                    DivideFileData(path, opt);
                }
                else
                {
                    DivideMemoryData(((List<double[]> input, List<double[]> target)?)o, opt);
                }
            });
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
            DivideFileDataCommand.RaiseCanExecuteChanged();
            DivideMemoryDataCommand.RaiseCanExecuteChanged();
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
            return (Vm!.TrainingSetPercent > 0 &&
                    Vm!.TrainingSetPercent + Vm!.ValidationSetPercent + Vm!.TestSetPercent == 100) ||
                (Vm!.TrainingSetPercent == 33 && Vm!.ValidationSetPercent == 33 && Vm!.TestSetPercent == 33);
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

        private void DivideMemoryData((List<double[]> input, List<double[]> target)? args, DataSetDivisionOptions? options = null)
        {
            IDataSetDivider method = Vm!.DivisionMethod switch
            {
                DivisionMethod.Random => new RandomDataSetDivider(),
                DivisionMethod.Normal => new LinearDataSetDivider(),
                _ => throw new NotImplementedException()
            };

            var pos = args!.Value!.input.Select((doubles, i) => (long) i).ToList();

            var opt = options ?? ConstructDivOptions();
            var divs = method.Divide(pos, opt);

            SupervisedTrainingSamples? training = null;
            SupervisedTrainingSamples? test = null;
            SupervisedTrainingSamples? validation = null;

            foreach (var div in divs)
            {
                var input = args.Value.input.Where((doubles, i) => div.positions.Contains(i)).ToArray();
                var target = args.Value.target.Where((doubles, i) => div.positions.Contains(i)).ToArray();

                if (div.setType == DataSetType.Training)
                {
                    training = SupervisedTrainingSamples.FromArrays(input, target);
                }

                if (div.setType == DataSetType.Test)
                {
                    test = SupervisedTrainingSamples.FromArrays(input, target);
                }

                if (div.setType == DataSetType.Validation)
                {
                    validation = SupervisedTrainingSamples.FromArrays(input, target);
                }
            }

            if (training == null) throw new Exception("Training set not present after data set division");

            var sets = new SupervisedTrainingData(training)
            {
                TestSet = test,
                ValidationSet = validation
            };

            var total = sets.TrainingSet.Input.Count + (sets.ValidationSet?.Input.Count ?? 0) +
                        (sets.TestSet?.Input.Count ?? 0);
            var calcTrainingPerc = (int) Math.Round(sets.TrainingSet.Input.Count * 100.0 / total);
            var calcValidationPerc = (int) Math.Round((sets.ValidationSet?.Input.Count ?? 0) * 100.0 / total);
            var calcTestPerc = (int) Math.Round((sets.TestSet?.Input.Count ?? 0) * 100.0 / total);

            Vm!.PropertyChanged -= VmOnPropertyChanged;
            Vm!.TrainingSetPercent = calcTrainingPerc;
            Vm!.ValidationSetPercent = calcValidationPerc;
            Vm!.TestSetPercent = calcTestPerc;
            Vm!.InsufficientSizeMsg = default;
            Vm!.UpdateRatio();
            Vm!.PropertyChanged += VmOnPropertyChanged;

            _appState.ActiveSession!.TrainingData!.StoreNewSets(sets);
            _appState.ActiveSession.RaiseTrainingDataUpdated();
        }

        private void DivideFileData(string path, DataSetDivisionOptions? options = null)
        {
            var existingData = _appState.ActiveSession!.TrainingData!;
            var opt = options ?? ConstructDivOptions();
            var sets = _dataService.LoadSets(path, Vm!.DivisionMethod switch
            {
                DivisionMethod.Random => new RandomDataSetDivider(),
                DivisionMethod.Normal => new LinearDataSetDivider(),
                _ => throw new NotImplementedException()
            }, opt, existingData.Variables.Indexes);
            _appState.ActiveSession.TrainingData!.StoreNewSets(sets);
            _appState.ActiveSession.RaiseTrainingDataUpdated();

            Vm!.PropertyChanged -= VmOnPropertyChanged;
            Vm!.TrainingSetPercent = opt.TrainingSetPercent;
            Vm!.ValidationSetPercent = opt.ValidationSetPercent;
            Vm!.TestSetPercent = opt.TestSetPercent;
            Vm!.UpdateRatio();
            Vm!.PropertyChanged += VmOnPropertyChanged;
        }

        public void Initialize() { }

        public DelegateCommand<string> DivideFileDataCommand { get; set; }
        public DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }
        public DelegateCommand<object> Divide13Command { get; set; }
        public DelegateCommand<object> Divide7020Command { get; set; }
    }
}
