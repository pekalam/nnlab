using System.Data;
using System.Linq;
using Infrastructure.Domain;
using NNLib.Common;

namespace Data.Application.ViewModels.DataSource.Preview
{
    public class DataSetInstanceAccessor
    {
        private readonly DataTable _dataTable;
        private readonly TrainingData _trainingData;
        private SupervisedSet _set;

        public DataSetInstanceAccessor(TrainingData trainingData,
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

        public int Count => _set.Input.Count;

        private DataTable GetInstance(int index)
        {
            var inputInstance = _set.Input[index];
            var targetInstance = _set.Target[index];

            var row = new string[_trainingData.Variables.Length];

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

            _dataTable.Rows.Clear();
            _dataTable.Rows.Add(row);

            return _dataTable;
        }

        public DataTable this[int index]
        {
            get => GetInstance(index);
        }
    }
}