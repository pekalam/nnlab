using System.ComponentModel;
using System.Diagnostics;
using Common.Domain;
using NNLib.Common;
using NNLib.Data;
using OxyPlot;
using OxyPlot.Series;

namespace Data.Application.ViewModels.DataSource.Statistics
{
    public class VariablesPlotController
    {
        private readonly VariablesPlotViewModel _vm;
        private static OxyPalette _pallete;

        static VariablesPlotController()
        {
            _pallete = new OxyPalette(new[]
            {
                OxyColor.FromRgb(43, 3, 192), OxyColor.FromRgb(126, 222, 20), OxyColor.FromRgb(243, 99, 6),
                OxyColor.FromRgb(34, 67, 2), OxyColor.FromRgb(81, 196, 250),
                OxyColor.FromRgb(121, 13, 26), OxyColor.FromRgb(39, 148, 124), OxyColor.FromRgb(123, 117, 126),
                OxyColor.FromRgb(248, 190, 18), OxyColor.FromRgb(234, 188, 184), OxyColor.FromRgb(129, 85, 1),
                OxyColor.FromRgb(244, 55, 101), OxyColor.FromRgb(16, 113, 243), OxyColor.FromRgb(22, 78, 83),
                OxyColor.FromRgb(189, 202, 183), OxyColor.FromRgb(129, 138, 42), OxyColor.FromRgb(166, 38, 135),
                OxyColor.FromRgb(194, 193, 250), OxyColor.FromRgb(65, 54, 83), OxyColor.FromRgb(39, 221, 212),
                OxyColor.FromRgb(171, 139, 110), OxyColor.FromRgb(156, 70, 251), OxyColor.FromRgb(71, 57, 41),
                OxyColor.FromRgb(26, 130, 152), OxyColor.FromRgb(98, 110, 93), OxyColor.FromRgb(8, 124, 32),
                OxyColor.FromRgb(153, 90, 74), OxyColor.FromRgb(186, 127, 159), OxyColor.FromRgb(13, 72, 127),
                OxyColor.FromRgb(7, 211, 138), OxyColor.FromRgb(255, 183, 125), OxyColor.FromRgb(116, 97, 153),
                OxyColor.FromRgb(190, 189, 102), OxyColor.FromRgb(142, 160, 172), OxyColor.FromRgb(184, 137, 6),
                OxyColor.FromRgb(238, 5, 26), OxyColor.FromRgb(54, 166, 9), OxyColor.FromRgb(228, 131, 117),
                OxyColor.FromRgb(245, 24, 168), OxyColor.FromRgb(118, 67, 83), OxyColor.FromRgb(144, 139, 187),
                OxyColor.FromRgb(254, 174, 222), OxyColor.FromRgb(131, 150, 123), OxyColor.FromRgb(95, 98, 10),
                OxyColor.FromRgb(119, 50, 3), OxyColor.FromRgb(186, 13, 72), OxyColor.FromRgb(79, 83, 89),
                OxyColor.FromRgb(83, 171, 178), OxyColor.FromRgb(18, 107, 80), OxyColor.FromRgb(98, 158, 251),
                OxyColor.FromRgb(121, 104, 87), OxyColor.FromRgb(95, 26, 115), OxyColor.FromRgb(189, 183, 192),
                OxyColor.FromRgb(76, 79, 201), OxyColor.FromRgb(52, 62, 47), OxyColor.FromRgb(164, 206, 213),
                OxyColor.FromRgb(179, 98, 24), OxyColor.FromRgb(201, 181, 163), OxyColor.FromRgb(183, 99, 195),
                OxyColor.FromRgb(136, 184, 119), OxyColor.FromRgb(66, 60, 5), OxyColor.FromRgb(134, 130, 251),
                OxyColor.FromRgb(117, 6, 73), OxyColor.FromRgb(253, 108, 160), OxyColor.FromRgb(164, 121, 119),
                OxyColor.FromRgb(228, 136, 18), OxyColor.FromRgb(178, 44, 2), OxyColor.FromRgb(148, 144, 138),
                OxyColor.FromRgb(186, 86, 120), OxyColor.FromRgb(59, 99, 120), OxyColor.FromRgb(136, 117, 64),
                OxyColor.FromRgb(112, 185, 163), OxyColor.FromRgb(198, 155, 222), OxyColor.FromRgb(199, 167, 106),
                OxyColor.FromRgb(99, 127, 77), OxyColor.FromRgb(24, 139, 200), OxyColor.FromRgb(97, 78, 76),
                OxyColor.FromRgb(129, 63, 174), OxyColor.FromRgb(101, 119, 159), OxyColor.FromRgb(127, 98, 120),
                OxyColor.FromRgb(103, 138, 138), OxyColor.FromRgb(1, 166, 100), OxyColor.FromRgb(56, 49, 121),
                OxyColor.FromRgb(10, 68, 47), OxyColor.FromRgb(215, 140, 94), OxyColor.FromRgb(168, 202, 235),
                OxyColor.FromRgb(55, 217, 242), OxyColor.FromRgb(202, 104, 71), OxyColor.FromRgb(57, 116, 113),
                OxyColor.FromRgb(87, 49, 42), OxyColor.FromRgb(175, 159, 1), OxyColor.FromRgb(111, 66, 115),
                OxyColor.FromRgb(175, 153, 151), OxyColor.FromRgb(85, 83, 57), OxyColor.FromRgb(153, 145, 102),
                OxyColor.FromRgb(226, 7, 245), OxyColor.FromRgb(110, 74, 51), OxyColor.FromRgb(216, 202, 6),
                OxyColor.FromRgb(222, 152, 165)
            });
        }


        public VariablesPlotController(VariablesPlotViewModel vm)
        {
            _vm = vm;
        }

        public void Plot(TrainingData args, DataSetType dataSetType)
        {
            int pal = 0;

            LineSeries createLineSeries(int varInd)
            {
                pal %= _pallete.Colors.Count;
                return new LineSeries
                {
                    StrokeThickness = 1,
                    Color = _pallete.Colors[pal++],
                    Title = args.Variables.Names[varInd],
                };
            }


            _vm.PlotModel.Model.Series.Clear();

            // if (args.GetSet(dataSetType)!.Input.Count > 5000)
            // {
            //     //Log.Logger.Debug("Ignoring loading plot for more than 5000 items (actual count: {count})", args.GetSet(dataSetType).Input.Count);
            //     return;
            // }

            if (_vm.SelectedVariablePlotType == VariablePlotType.Input)
            {
                for (int i = 0; i < args.Variables.Indexes.InputVarIndexes.Length; i++)
                {
                    var s = createLineSeries(args.Variables.Indexes.InputVarIndexes[i]);

                    for (int j = 0; j < args.GetSet(dataSetType)!.Input.Count; j++)
                    {
                        s.Points.Add(new DataPoint(j, args.GetSet(dataSetType)!.Input[j][i, 0]));
                    }

                    _vm.PlotModel.Model.Series.Add(s);
                }
            }

            if (_vm.SelectedVariablePlotType == VariablePlotType.Target)
            {
                for (int i = 0; i < args.Variables.Indexes.TargetVarIndexes.Length; i++)
                {
                    var s = createLineSeries(args.Variables.Indexes.TargetVarIndexes[i]);

                    for (int j = 0; j < args.GetSet(dataSetType)!.Target.Count; j++)
                    {
                        s.Points.Add(new DataPoint(j, args.GetSet(dataSetType)!.Target[j][i, 0]));
                    }

                    _vm.PlotModel.Model.Series.Add(s);
                }
            }

            _vm.PlotModel.Model.InvalidatePlot(true);
        }
    }
}