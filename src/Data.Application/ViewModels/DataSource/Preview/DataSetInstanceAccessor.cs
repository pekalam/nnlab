using NNLib.Common;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Common.Domain;
using NNLib.Data;

namespace Data.Application.ViewModels.DataSource.Preview
{
    public class DataSetInstanceAccessor
    {
        private readonly DataTable _dataTable;
        private readonly AppState _appState;
        private SupervisedTrainingSamples _set;


        public DataSetInstanceAccessor(AppState appState, DataSetType defaultDataSetType = DataSetType.Training)
        {
            Debug.Assert(appState.ActiveSession?.TrainingData != null);
            _appState = appState;
            var trainingData = _appState.ActiveSession!.TrainingData!;
            _set = trainingData.GetSet(defaultDataSetType)!;

            _dataTable = new DataTable();
            var cols = new DataColumn[trainingData.Variables.Names.Length - trainingData.Variables.Indexes.Ignored.Length];
            int ind = 0;
            for (int i = 0; i < trainingData.Variables.Names.Length; i++)
            {
                if (!trainingData.Variables.Indexes.Ignored.Contains(i))
                {
                    var name = trainingData.Variables.Names[i].ToString().Replace('.', '_');
                    cols[ind++] = new DataColumn(name);
                    _dataTable.ExtendedProperties[name] = trainingData.Variables.Indexes.InputVarIndexes.Contains(i) ? VariableUses.Input : VariableUses.Target;
                }
            }
            _dataTable.Columns.AddRange(cols);
        }

        public void ChangeDataSet(DataSetType type)
        {
            _set = _appState.ActiveSession!.TrainingData!.GetSet(type)!;
        }

        public int Count => _set.Input.Count;

        private DataTable GetInstance(int index)
        {
            var inputInstance = _set.Input[index];
            var targetInstance = _set.Target[index];
            var trainingData = _appState.ActiveSession!.TrainingData!;

            var row = new string[trainingData.Variables.Length];

            int i = 0;
            foreach (var inputVarIndex in trainingData.Variables.Indexes.InputVarIndexes)
            {
                row[inputVarIndex] = inputInstance[i++, 0].ToString();
            }

            i = 0;
            foreach (var targetVarIndex in trainingData.Variables.Indexes.TargetVarIndexes)
            {
                row[targetVarIndex] = targetInstance[i++, 0].ToString();
            }

            _dataTable.Rows.Clear();
            _dataTable.Rows.Add(row.Where(v => v != null).ToArray());

            return _dataTable;
        }

        public DataTable this[int index]
        {
            get => GetInstance(index);
        }
    }
}