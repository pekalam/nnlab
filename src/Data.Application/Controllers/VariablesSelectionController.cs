using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using Data.Domain;
using Data.Domain.Services;
using Prism.Commands;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using NNLib.Csv;
using Prism.Events;
using Shell.Interface;

namespace Data.Application.Controllers
{
    public interface IVariablesSelectionController : IController
    {
        ICommand IgnoreAllCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<IVariablesSelectionController, VariablesSelectionController>();
        }
    }

    internal class VariablesSelectionController : ControllerBase<VariablesSelectionViewModel>,IVariablesSelectionController
    {
        private readonly AppState _appState;
        private SupervisedSetVariableIndexes _currentIndexes = null!;
        private readonly ITrainingDataService _dsService;
        private readonly INormalizationDomainService _normalizationService;

        public VariablesSelectionController(AppState appState, ITrainingDataService dsService, INormalizationDomainService normalizationService, IEventAggregator ea)
        {
            _appState = appState;
            _dsService = dsService;
            _normalizationService = normalizationService;

            IgnoreAllCommand = new DelegateCommand(IgnoreAll);

            ea.GetEvent<PreviewCheckNavMenuItem>().Subscribe(args =>
            {
                ea.GetEvent<HideFlyout>().Publish();
            });
        }

        public ICommand IgnoreAllCommand { get; set; }

        protected override void VmCreated()
        {
            Vm!.IsActiveChanged += VmOnIsActiveChanged;
        }

        private void VmOnIsActiveChanged(object? sender, EventArgs e)
        {
            if(!Vm!.IsActive) return;

            Debug.Assert(_appState.ActiveSession?.TrainingData != null);

            _currentIndexes = _appState.ActiveSession!.TrainingData!.Variables.Indexes;

            InitVmWithData();
        }

        private void InitVmWithData()
        {
            var trainingData = _appState.ActiveSession!.TrainingData!;

            var variables = VariableTableModel.FromTrainingData(trainingData);

            foreach (var model in variables)
            {
                model.OnVariableUseSet = OnVariableUseSet;
            }

            Vm!.Variables = variables;
        }

        private async void OnVariableUseSet(VariableTableModel model)
        {
            var inputIndexes = new List<int>();
            var targetIndexes = new List<int>();
            var ignoredIndexes = new List<int>();
            foreach (var var in model.ContainingArray!)
            {
                if(var[nameof(VariableTableModel.VariableUse)] != null) return;
                if (var.VariableUse == VariableUses.Input) inputIndexes.Add(var.Index);
                if (var.VariableUse == VariableUses.Target) targetIndexes.Add(var.Index);
                if (var.VariableUse == VariableUses.Ignore) ignoredIndexes.Add(var.Index);
            }


            var trainingData = _appState.ActiveSession!.TrainingData!;
            var newIndexes = new SupervisedSetVariableIndexes(inputIndexes.ToArray(), targetIndexes.ToArray(), ignoredIndexes.ToArray());


            if (newIndexes.InputVarIndexes.SequenceEqual(_currentIndexes.InputVarIndexes) && newIndexes.TargetVarIndexes.SequenceEqual(_currentIndexes.TargetVarIndexes) && newIndexes.Ignored.SequenceEqual(_currentIndexes.Ignored)) return;

            _currentIndexes = newIndexes;

            var normalization = trainingData.NormalizationMethod;

            if (normalization != NormalizationMethod.None)
            {
                _dsService.ChangeVariables(_currentIndexes, trainingData.OriginalSets, trainingData.Source);
                await _normalizationService.Normalize(normalization);
                _dsService.ChangeVariables(_currentIndexes, trainingData);
            }
            else
            {
                _dsService.ChangeVariables(_currentIndexes, trainingData.OriginalSets, trainingData.Source);
                _dsService.ChangeVariables(_currentIndexes, trainingData);
            }
        }

        private void IgnoreAll()
        {
            foreach (var model in Vm!.Variables)
            {
                model.OnVariableUseSet = null;
            }

            foreach (var model in Vm!.Variables)
            {
                model.VariableUse = VariableUses.Ignore;
            }

            foreach (var model in Vm!.Variables)
            {
                model.OnVariableUseSet = OnVariableUseSet;
            }
        }
    }
}
