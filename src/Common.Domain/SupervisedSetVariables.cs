using System;
using System.Collections.Generic;
using System.Linq;
using NNLib.Common;

namespace Common.Domain
{
    public class SupervisedSetVariables
    {
        private readonly SupervisedSetVariableIndexes _variableIndexes;
        private readonly VariableName[] _variableNames;

        public SupervisedSetVariables(SupervisedSetVariableIndexes variableIndexes, VariableName[] variableNames)
        {
            ValidateCtorParams(variableIndexes, variableNames);
            _variableIndexes = variableIndexes;
            _variableNames = variableNames;
        }

        public SupervisedSetVariableIndexes Indexes => _variableIndexes;
        public VariableName[] Names => _variableNames;
        public int Length => _variableNames.Length;

        private void ValidateCtorParams(SupervisedSetVariableIndexes variableIndexes, VariableName[] variableNames)
        {
            if (variableNames.Length == 0)
            {
                throw new ArgumentException("Empty variableNames array");
            }
            if (variableIndexes.Ignored.Length + variableIndexes.InputVarIndexes.Length + variableIndexes.TargetVarIndexes.Length !=
                variableNames.Length)
            {
                throw new ArgumentException(
                    $"variableNames.Length has different length than TargetVarIndexes.Length ({variableIndexes.TargetVarIndexes.Length}) + InputVarIndexes.Length {variableIndexes.InputVarIndexes.Length}");
            }
        }

        public string[] InputVariableNames
        {
            get
            {
                var names = new List<string>();
                for (int i = 0; i < Indexes.InputVarIndexes.Length; i++)
                {
                    names.Add(_variableNames[Indexes.InputVarIndexes[i]]);
                }

                return names.ToArray();
            }
        }

        public string[] TargetVariableNames
        {
            get
            {
                var names = new List<string>();
                for (int i = 0; i < Indexes.TargetVarIndexes.Length; i++)
                {
                    names.Add(_variableNames[Indexes.TargetVarIndexes[i]]);
                }

                return names.ToArray();
            }
        }

        public SupervisedSetVariables Clone() => new SupervisedSetVariables(_variableIndexes.Clone(), _variableNames.Select(v => new VariableName(v.ToString())).ToArray());
    }
}