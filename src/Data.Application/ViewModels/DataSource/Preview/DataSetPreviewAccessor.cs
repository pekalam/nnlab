using NNLib.Common;
using System.Data;
using Common.Domain;

namespace Data.Application.ViewModels.DataSource.Preview
{
    public class DataSetPreviewAccessor
    {
        private SupervisedSet _set;
        private readonly DataTable _dataTable;
        private readonly TrainingData _trainingData;

        public DataSetPreviewAccessor(TrainingData trainingData,
            DataSetType defaultDataSetType = DataSetType.Training)
        {
            _trainingData = trainingData;
            _set = trainingData.GetSet(defaultDataSetType)!;
            _dataTable = new DataTable();


            var cols = new DataColumn[trainingData.Variables.Names.Length - trainingData.Variables.Indexes.Ignored.Length];
            int ind = 0;
            for (int i = 0; i < trainingData.Variables.Names.Length; i++)
            {
                if (!trainingData.Variables.Indexes.Ignored.Contains(i))
                {
                    cols[ind++] = new DataColumn(trainingData.Variables.Names[i].ToString().Replace('.', '_'));
                }
            }
            _dataTable.Columns.AddRange(cols);
        }

        public void ChangeDataSet(DataSetType type)
        {
            _set = _trainingData.GetSet(type)!;
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

                var row = new string[_dataTable.Columns.Count];

                int i = 0;
                foreach (var inputVarIndex in _trainingData.Variables.Indexes.InputVarIndexes)
                {
                    row[inputVarIndex] = inputInstance[i++, 0].ToString();
                }

                i = 0;
                foreach (var targetVarIndex in _trainingData.Variables.Indexes.TargetVarIndexes)
                {
                    row[targetVarIndex] = targetInstance[i++, 0].ToString();
                }

                _dataTable.Rows.Add(row);
            }

            return _dataTable;
        }
    }
}