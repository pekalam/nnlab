using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Data.Application.Services;
using Data.Application.ViewModels;
using Infrastructure.Domain;
using NNLib.Common;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;

namespace Data.Application.Controllers
{
    internal class CustomDataSetController
    {
        private readonly CustomDataSetService _dsService;
        private readonly AppState _appState;
        private readonly List<double[]> _input = new List<double[]>();
        private readonly List<double[]> _target = new List<double[]>();

        public CustomDataSetController(CustomDataSetService dsService, AppState appState)
        {
            _dsService = dsService;
            _appState = appState;
            _dsService.PlotMouseDownCommand = new DelegateCommand<OxyMouseDownEventArgs>(PlotMouseDown);

            CustomDataSetViewModel.Created += () =>
            {
                //create session when navigated
                _appState.SessionManager.Create();
            };
        }

        private void PlotMouseDown(OxyMouseDownEventArgs args)
        {
            if (args.ChangedButton == OxyMouseButton.Left && args.ClickCount == 2)
            {
                var vm = CustomDataSetViewModel.Instance;

                var p = Axis.InverseTransform(args.Position, vm.PlotModel.Axes[0], vm.PlotModel.Axes[1]);

                AddPoint(p);
                vm.Scatter.Points.Add(new ScatterPoint(p.X, p.Y));

                vm.Line.Points.Clear();
                var sortedPoints = vm.Scatter.Points.Select(p => new DataPoint(p.X, p.Y)).OrderBy(p => p.X);
                vm.Line.Points.AddRange(sortedPoints);
                vm.PlotModel.InvalidatePlot(false);
            }
        }


        private void SortPoints()
        {
            for (int i = _input.Count; i > 0; i--)
            {
                for (int j = 1; j < i; j++)
                {
                    if (_input[j][0] < _input[j - 1][0])
                    {
                        var tmp = _input[j];
                        _input[j] = _input[j - 1];
                        _input[j - 1] = tmp;

                        tmp = _target[j];
                        _target[j] = _target[j - 1];
                        _target[j - 1] = tmp;
                    }
                }
            }
        }

        private void AddPoint(DataPoint p)
        {
            _input.Add(new[] { p.X });
            _target.Add(new[] { p.Y });

            SortPoints();

            if (_input.Count >= 3)
            {
                var sets = new SupervisedTrainingSets(SupervisedSet.FromArrays(_input.ToArray(),
                    _target.ToArray()));

                var trainingData = new TrainingData(sets,
                    new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }),
                        new[] { new VariableName("x"), new VariableName("y"), }), TrainingDataSource.Memory);

                _appState.SessionManager.ActiveSession.TrainingData = trainingData;
            }
        }
    }
}
