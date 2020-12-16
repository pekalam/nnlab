using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using Data.Domain;
using Data.Domain.Services;
using NNLib.Data;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using NNLib.Csv;
using Prism.Events;
using Shell.Interface;

namespace Data.Application.Controllers
{
    public interface IDataSetDivisionController : IController
    {
        DelegateCommand<string> DivideFileDataCommand { get; set; }
        DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }

        DelegateCommand<object> Divide13Command { get; set; }
        DelegateCommand<object> Divide7020Command { get; set; }

        bool ValidPercents();

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IDataSetDivisionController, DataSetDivisionController>();
        }
    }

    internal class DataSetDivisionController : ControllerBase<DataSetDivisionViewModel>,IDataSetDivisionController
    {
        private readonly AppState _appState;
        private readonly ITrainingDataService _dataService;

        public DataSetDivisionController( ITrainingDataService dataService, AppState appState, IEventAggregator ea)
        {
            _dataService = dataService;
            _appState = appState;

            ea.GetEvent<PreviewCheckNavMenuItem>().Subscribe(args =>
            {
                ea.GetEvent<HideFlyout>().Publish();
            });

            bool CanExecuteDivide()
            {
                bool canExec = ValidPercents();
                if (canExec && Vm != null)
                {
                    return Vm.InsufficientSizeMsg == default;
                }

                return canExec;
            }

            DivideFileDataCommand =
                new DelegateCommand<string>(s => DivideFileData(s), _ => CanExecuteDivide());

            DivideMemoryDataCommand = new DelegateCommand<(List<double[]> input, List<double[]> target)?>(args => DivideMemoryData(args), _ => CanExecuteDivide());


            Divide13Command = new DelegateCommand<object>(o =>
            {
                var opt = new DataSetDivisionOptions()
                {
                    TrainingSetPercent = Vm!.TrainingSetPercent = 33.33m,
                    TestSetPercent = Vm!.TestSetPercent = 33.33m,
                    ValidationSetPercent = Vm!.ValidationSetPercent = 33.33m,
                };

                if (Vm!.InsufficientSizeMsg != default || Vm!.RatioModificationMsg != default) return;

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
                var opt = new DataSetDivisionOptions()
                {
                    TrainingSetPercent = Vm!.TrainingSetPercent = 70,
                    ValidationSetPercent = Vm!.ValidationSetPercent = 20,
                    TestSetPercent = Vm!.TestSetPercent = 10,
                };

                if(Vm!.InsufficientSizeMsg != default || Vm!.RatioModificationMsg != default) return;

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
            if (_appState.ActiveSession?.TrainingData != null)
            {
                var data = _appState.ActiveSession.TrainingData;
                decimal total = data.Sets.TrainingSet.Input.Count + (data.Sets.ValidationSet?.Input.Count ?? 0) + (data.Sets.TestSet?.Input.Count ?? 0);

                var t = data.Sets.TrainingSet.Input.Count * 100.0m / total;
                var ts = data.Sets.TestSet?.Input.Count * 100.0m / total ?? 0;
                var v = data.Sets.ValidationSet?.Input.Count * 100.0m / total ?? 0;

                Vm!.UpdateRatio(t, v, ts);
            }


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

        private void SetExactRatio(SupervisedTrainingData sets)
        {
            var total = sets.TrainingSet.Input.Count + (sets.ValidationSet?.Input.Count ?? 0) +
                        (sets.TestSet?.Input.Count ?? 0);

            var calcTrainingPerc = sets.TrainingSet.Input.Count * 100.0m / total;
            var calcValidationPerc = (sets.ValidationSet?.Input.Count ?? 0) * 100.0m / total;
            var calcTestPerc = (sets.TestSet?.Input.Count ?? 0) * 100.0m / total;
            Vm!.UpdateRatio(calcTrainingPerc, calcValidationPerc, calcTestPerc);
        }

        private void CalcHasSufficientSize()
        {
            DivideFileDataCommand.RaiseCanExecuteChanged();
            DivideMemoryDataCommand.RaiseCanExecuteChanged();
            if (!ValidPercents()) return;

            if (_appState.ActiveSession?.TrainingData != null)
            {
                var data = _appState.ActiveSession.TrainingData;
                var total = data.Sets.TrainingSet.Input.Count + (data.Sets.ValidationSet?.Input.Count ?? 0) + (data.Sets.TestSet?.Input.Count ?? 0);
                var left = total;

                var trainingSetCount = (int)Math.Ceiling(Vm!.TrainingSetPercent * total / 100m);
                trainingSetCount = trainingSetCount > left ? left : trainingSetCount;
                left -= trainingSetCount;

                var validationSetCount = (int)Math.Ceiling(Vm!.ValidationSetPercent * total / 100m);
                validationSetCount = validationSetCount > left ? left : validationSetCount;
                left -= validationSetCount;

                var testSetCount = (int)Math.Ceiling(Vm!.TestSetPercent * total / 100m);
                testSetCount = testSetCount > left ? left : testSetCount;

                var calcTrainingPerc = Math.Round(trainingSetCount * 100.0m / total, 2);
                var calcValidationPerc = Math.Round(validationSetCount * 100.0m / total, 2);
                var calcTestPerc = Math.Round(testSetCount * 100.0m / total, 2);

                if (trainingSetCount == 0 || (validationSetCount == 0 && Vm!.ValidationSetPercent > 0) || (testSetCount == 0 && Vm!.TestSetPercent > 0))
                {
                    Vm!.InsufficientSizeMsg = $"Data cannot be divided by ratio: {Vm!.TrainingSetPercent}%:{Vm!.ValidationSetPercent}%:{Vm!.TestSetPercent}%";
                    Vm!.RatioModificationMsg = default;
                }
                else if ((calcTrainingPerc != Vm!.TrainingSetPercent || calcValidationPerc != Vm!.ValidationSetPercent || calcTestPerc != Vm!.TestSetPercent))
                {
                    Vm!.RatioModificationMsg = $"Ratio after division: {calcTrainingPerc}%:{calcValidationPerc}%:{calcTestPerc}%";
                    Vm!.InsufficientSizeMsg = default;
                }
                else
                {
                    Vm!.InsufficientSizeMsg = default;
                    Vm!.RatioModificationMsg = default;
                }
                DivideFileDataCommand.RaiseCanExecuteChanged();
                DivideMemoryDataCommand.RaiseCanExecuteChanged();
            }

        }

        public bool ValidPercents()
        {
            return (Vm!.TrainingSetPercent > 0 &&
                    Vm!.TrainingSetPercent + Vm!.ValidationSetPercent + Vm!.TestSetPercent == 100m) ||
                (Vm!.TrainingSetPercent == 33.33m && Vm!.ValidationSetPercent == 33.33m && Vm!.TestSetPercent == 33.33m) || 
                (Vm!.TrainingSetPercent % 1 == .33m && Vm!.ValidationSetPercent % 1 == .33m && Vm!.TestSetPercent % 1 == .33m);
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

            Vm!.PropertyChanged -= VmOnPropertyChanged;
            SetExactRatio(sets);
            Vm!.RatioModificationMsg = default;
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
            SetExactRatio(sets);
            Vm!.RatioModificationMsg = default;
            Vm!.PropertyChanged += VmOnPropertyChanged;
        }

        public DelegateCommand<string> DivideFileDataCommand { get; set; }
        public DelegateCommand<(List<double[]> input, List<double[]> target)?> DivideMemoryDataCommand { get; set; }
        public DelegateCommand<object> Divide13Command { get; set; }
        public DelegateCommand<object> Divide7020Command { get; set; }
    }
}
