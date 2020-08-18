using System.Data;
using System.Linq;
using Infrastructure.Domain;
using NNLib.Common;

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
            _set = trainingData.GetSet(defaultDataSetType);
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
            _set = _trainingData.GetSet(type);
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

                var rows = new string[_dataTable.Columns.Count];

                int i = 0;
                for (int k = 0; k < _trainingData.Variables.Indexes.InputVarIndexes.Length; k++)
                {
                    rows[i++] = inputInstance[k, 0].ToString();
                }

                for (int k = 0; k < _trainingData.Variables.Indexes.TargetVarIndexes.Length; k++)
                {
                    rows[i++] = targetInstance[k, 0].ToString();
                }

                _dataTable.Rows.Add(rows.Where(r => r != null).ToArray());
            }

            return _dataTable;
        }
    }
}