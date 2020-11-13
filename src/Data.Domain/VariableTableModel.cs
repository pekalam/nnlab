using Common.Domain;
using NNLib.Common;
using Prism.Mvvm;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Data.Domain
{
    public class VariableTableModel : BindableBase, IDataErrorInfo
    {
        private const string NoTargetVariableErrorMsg = "No target variable";
        private const string NoInputVariableErrorMsg = "No input variable";
        private const string TwoTargetVariablesErrorMsg = "Two target variables are not supported";

        private VariableUses _variableUse;

        private VariableTableModel(VariableUses initialVariableUse)
        {
            _variableUse = initialVariableUse;
        }

        public static VariableTableModel[] FromTrainingData(TrainingData trainingData)
        {
            var variables = trainingData.Variables.Names
                .Select((v, ind) =>
                {
                    VariableUses use;
                    if (trainingData.Variables.Indexes.InputVarIndexes.Contains(ind))
                    {
                        use = VariableUses.Input;
                    }
                    else if (trainingData.Variables.Indexes.TargetVarIndexes.Contains(ind))
                    {
                        use = VariableUses.Target;
                    }
                    else
                    {
                        use = VariableUses.Ignore;
                    }

                    return new VariableTableModel(use)
                    {
                        Name = v,
                        Index = ind,
                    };
                }).ToArray();

            foreach (var model in variables)
            {
                model.ContainingArray = variables;
            }

            return variables;
        }

        public VariableTableModel[] ContainingArray { get; private set; } = null!;

        public int Index { get; private set; }
        public string Name { get; set; } = null!;

        public VariableUses VariableUse
        {
            get => _variableUse;
            set
            {
                Debug.Assert(ContainingArray != null);
                SetProperty(ref _variableUse, value);
                foreach (var model in ContainingArray)
                {
                    if(model != this && model.Error != null) model.RaisePropertyChanged(nameof(VariableUse));
                }
                if(!CheckHasNoInputVars() && !CheckHasNoTargetVars() && !CheckHasOneTargetVar()) OnVariableUseSet?.Invoke(this);
            }
        }

        private bool CheckHasNoInputVars() => VariableUse != VariableUses.Input &&
                                            ContainingArray.All(v => v.VariableUse != VariableUses.Input);

        private bool CheckHasNoTargetVars() => VariableUse != VariableUses.Target &&
                                             ContainingArray.All(v => v.VariableUse != VariableUses.Target);

        private bool CheckHasOneTargetVar() => VariableUse == VariableUses.Target &&
                                               ContainingArray.Any(v => v != this && v.VariableUse == VariableUses.Target);

        public string? Error { get; private set; }
        public string? this[string columnName]
        {
            get
            {
                if (CheckHasNoInputVars())
                {
                    OnError?.Invoke(NoInputVariableErrorMsg);
                    return Error = NoInputVariableErrorMsg;
                }
                if (CheckHasNoTargetVars())
                {
                    OnError?.Invoke(NoTargetVariableErrorMsg);
                    return Error = NoTargetVariableErrorMsg;
                }

                if (CheckHasOneTargetVar())
                {
                    OnError?.Invoke(TwoTargetVariablesErrorMsg);
                    return Error = TwoTargetVariablesErrorMsg;
                }

                OnError?.Invoke(null);
                return Error = null;
            }
        }

        public Action<string?>? OnError;
        public Action<VariableTableModel>? OnVariableUseSet;
    }
}