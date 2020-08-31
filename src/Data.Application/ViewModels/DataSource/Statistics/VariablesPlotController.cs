using System.Diagnostics;
using Common.Domain;
using NNLib.Common;
using OxyPlot;
using OxyPlot.Series;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class VariablesPlotController
    {
        private readonly VariablesPlotViewModel _vm;
        private readonly OxyPalette _pallete;

        public VariablesPlotController(VariablesPlotViewModel vm)
        {
            _vm = vm;
            //TODO pallete
            _pallete = OxyPalettes.Rainbow(100);
        }

        public void Plot(TrainingData args, DataSetType dataSetType)
        {
            int pal = 0;
            LineSeries createLineSeries(int varInd)
            {
                return new LineSeries
                {
                    StrokeThickness = 1,
                    Color = _pallete.Colors[pal++],
                    Title = args.Variables.Names[varInd],
                };
            }


            _vm.PlotModel.Series.Clear();
            
            if (args.GetSet(dataSetType)!.Input.Count > 5000)
            {
                //Log.Logger.Debug("Ignoring loading plot for more than 5000 items (actual count: {count})", args.GetSet(dataSetType).Input.Count);
                return;
            }

            for (int i = 0; i < args.Variables.Indexes.InputVarIndexes.Length; i++)
            {
                var s = createLineSeries(args.Variables.Indexes.InputVarIndexes[i]);

                for (int j = 0; j < args.GetSet(dataSetType)!.Input.Count; j++)
                {
                    s.Points.Add(new DataPoint(j, args.GetSet(dataSetType)!.Input[j][i, 0]));
                }

                _vm.PlotModel.Series.Add(s);
            }

            for (int i = 0; i < args.Variables.Indexes.TargetVarIndexes.Length; i++)
            {
                var s = createLineSeries(args.Variables.Indexes.TargetVarIndexes[i]);

                for (int j = 0; j < args.GetSet(dataSetType)!.Target.Count; j++)
                {
                    s.Points.Add(new DataPoint(j, args.GetSet(dataSetType)!.Target[j][i, 0]));
                }

                _vm.PlotModel.Series.Add(s);
            }

            _vm.PlotModel.InvalidatePlot(true);
        }
    }
}