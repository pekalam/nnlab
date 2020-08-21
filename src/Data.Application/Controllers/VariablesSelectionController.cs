using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.VariablesSelection;
using Data.Domain;
using Data.Domain.Services;
using NNLib.Common;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Common.Domain;
using Data.Application.Controllers;
using Prism.Ioc;

namespace Data.Application.Services
{
    public interface IVariablesSelectionService
    {
        ICommand IgnoreAllCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IVariablesSelectionService, VariablesSelectionController>();
        }
    }
}

namespace Data.Application.Controllers
{
    internal class VariablesSelectionController : IVariablesSelectionService, ITransientController
    {
        private AppState _appState;
        private SupervisedSetVariableIndexes _currentIndexes;
        private readonly ITrainingDataService _dsService;

        public VariablesSelectionController(AppState appState, ITrainingDataService dsService)
        {
            _appState = appState;
            _dsService = dsService;

            IgnoreAllCommand = new DelegateCommand(IgnoreAll);

            VariablesSelectionViewModel.Created += () =>
            {
                _currentIndexes = _appState.ActiveSession.TrainingData.Variables.Indexes;

                InitVmWithData();
            };
        }

        public ICommand IgnoreAllCommand { get; set; }

        private void InitVmWithData()
        {
            var trainingData = _appState.ActiveSession.TrainingData;
            var vm = VariablesSelectionViewModel.Instance;

            var variables = VariableTableModel.FromTrainingData(trainingData);

            foreach (var model in variables)
            {
                model.OnVariableUseSet = OnVariableUseSet;
            }

            vm.Variables = variables;
        }

        private void OnVariableUseSet(VariableTableModel model)
        {
            var inputIndexes = new List<int>();
            var targetIndexes = new List<int>();
            var ignoredIndexes = new List<int>();
            foreach (var var in model.ContainingArray)
            {
                if(var.Error != null) return;
                if (var.VariableUse == VariableUses.Input) inputIndexes.Add(var.Index);
                if (var.VariableUse == VariableUses.Target) targetIndexes.Add(var.Index);
                if (var.VariableUse == VariableUses.Ignore) ignoredIndexes.Add(var.Index);
            }


            var trainingData = _appState.ActiveSession.TrainingData;
            var newIndexes = new SupervisedSetVariableIndexes(inputIndexes.ToArray(), targetIndexes.ToArray(), ignoredIndexes.ToArray());


            if (newIndexes.InputVarIndexes.SequenceEqual(_currentIndexes.InputVarIndexes) && newIndexes.TargetVarIndexes.SequenceEqual(_currentIndexes.TargetVarIndexes) && newIndexes.Ignored.SequenceEqual(_currentIndexes.Ignored)) return;

            _currentIndexes = newIndexes;

            _dsService.ChangeVariables(_currentIndexes, trainingData);
        }

        private void IgnoreAll()
        {
            var vm = VariablesSelectionViewModel.Instance;

            foreach (var model in vm.Variables)
            {
                model.OnVariableUseSet = null;
            }

            foreach (var model in vm.Variables)
            {
                model.VariableUse = VariableUses.Ignore;
                if (model.Error == null)
                {
                    _currentIndexes = _currentIndexes.ChangeVariableUse(model.Index, model.VariableUse);
                }
            }

            var trainingData = _appState.ActiveSession.TrainingData;
            _dsService.ChangeVariables(_currentIndexes, trainingData);

            foreach (var model in vm.Variables)
            {
                model.OnVariableUseSet = OnVariableUseSet;
            }
        }

    }
}
