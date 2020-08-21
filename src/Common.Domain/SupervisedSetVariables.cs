using System;
using System.Collections.Generic;
using System.Linq;
using NNLib.Common;

namespace Common.Domain
{
    public class SupervisedSetVariables
    {
        public SupervisedSetVariables(SupervisedSetVariableIndexes variableIndexes, VariableName[] variableNames)
        {
            ValidateCtorParams(variableIndexes, variableNames);
            Indexes = variableIndexes;
            Names = variableNames;
        }

        public SupervisedSetVariableIndexes Indexes { get; }
        public VariableName[] Names { get; }
        public int Length => Names.Length;

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
                    names.Add(Names[Indexes.InputVarIndexes[i]]);
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
                    names.Add(Names[Indexes.TargetVarIndexes[i]]);
                }

                return names.ToArray();
            }
        }

        public SupervisedSetVariables Clone() => new SupervisedSetVariables(Indexes.Clone(), Names.Select(v => new VariableName(v.ToString())).ToArray());
    }
}