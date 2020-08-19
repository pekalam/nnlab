using System;
using System.ComponentModel;
using System.Linq;
using Infrastructure.Domain;
using NNLib.Common;
using Prism.Mvvm;

namespace Data.Domain
{
    public class VariableTableModel : BindableBase, IDataErrorInfo
    {
        private VariableUses _variableUse;

        public VariableTableModel(VariableUses initialVariableUse)
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

        public VariableTableModel[] ContainingArray { get; set; }

        public int Index { get; set; }
        public string Name { get; set; }

        public VariableUses VariableUse
        {
            get => _variableUse;
            set
            {
                SetProperty(ref _variableUse, value);
                foreach (var model in ContainingArray)
                {
                    if(model != this && model.Error != null) model.RaisePropertyChanged(nameof(VariableUse));
                }
                if(!CheckHasNoInputVars() && !CheckHasNoTargetVars()) OnVariableUseSet?.Invoke(this);
            }
        }

        private bool CheckHasNoInputVars() => VariableUse != VariableUses.Input &&
                                            ContainingArray.All(v => v.VariableUse != VariableUses.Input);

        private bool CheckHasNoTargetVars() => VariableUse != VariableUses.Target &&
                                             ContainingArray.All(v => v.VariableUse != VariableUses.Target);

        public string Error { get; private set; }
        public string this[string columnName]
        {
            get
            {
                if (CheckHasNoInputVars())
                {
                    OnError?.Invoke("No input variable");
                    return Error = "No input variable";
                }
                if (CheckHasNoTargetVars())
                {
                    OnError?.Invoke("No target variable");
                    return Error = "No target variable";
                }

                OnError?.Invoke(null);
                return Error = null;
            }
        }

        public Action<string> OnError;
        public Action<VariableTableModel> OnVariableUseSet;
    }
}