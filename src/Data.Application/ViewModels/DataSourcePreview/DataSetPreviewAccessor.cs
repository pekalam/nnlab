using Common.Domain;
using NNLib.Common;
using NNLib.Data;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Data.Application.ViewModels.DataSourcePreview
{
    public class DataSetPreviewAccessor
    {
        private SupervisedTrainingSamples _set;
        private readonly DataTable _dataTable;
        private readonly AppState _appState;

        public DataSetPreviewAccessor(AppState appState, DataSetType defaultDataSetType = DataSetType.Training)
        {
            _appState = appState;
            Debug.Assert(_appState.ActiveSession?.TrainingData != null);
            var trainingData = _appState.ActiveSession.TrainingData;
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

        public DataTable GetPreview(int instancesCount)
        {
            _dataTable.Rows.Clear();

            if (instancesCount > _set.Input.Count)
            {
                instancesCount = _set.Input.Count;
            }

            for (int j = 0; j < instancesCount; j++)
            {
                var inputInstance = _set.Input[j];
                var targetInstance = _set.Target[j];
                var trainingData = _appState.ActiveSession!.TrainingData!;

                var row = new string[trainingData.Variables.Length];

                int i = 0;
                foreach (var inputVarIndex in trainingData.Variables.Indexes.InputVarIndexes)
                {
                    row[inputVarIndex] = inputInstance[i++, 0].ToString(CultureInfo.InvariantCulture);
                }

                i = 0;
                foreach (var targetVarIndex in trainingData.Variables.Indexes.TargetVarIndexes)
                {
                    row[targetVarIndex] = targetInstance[i++, 0].ToString(CultureInfo.InvariantCulture);
                }

                _dataTable.Rows.Add(row.Where(v => v!=null).ToArray());
            }

            return _dataTable;
        }
    }
}