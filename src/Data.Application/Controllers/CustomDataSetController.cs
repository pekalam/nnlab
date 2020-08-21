using Common.Domain;
using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.CustomDataSet;
using Data.Application.ViewModels.DataSetDivision;
using Data.Application.ViewModels.DataSource.VariablesSelection;
using NNLib.Common;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using System.Collections.Generic;
using System.Linq;
using Data.Application.Controllers;
using Prism.Ioc;


namespace Data.Application.Services
{
    public interface ICustomDataSetService
    {
        DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        DelegateCommand OpenDivisionViewCommand { get; set; }
        DelegateCommand SelectVariablesCommand { get; set; }

        public static void Register(IContainerRegistry cr)
        {
            cr.Register<ICustomDataSetService, CustomDataSetController>();
        }
    }
}

namespace Data.Application.Controllers
{
    internal class CustomDataSetController : ICustomDataSetService, ITransientController
    {
        private readonly AppState _appState;
        private readonly List<double[]> _input = new List<double[]>();
        private readonly List<double[]> _target = new List<double[]>();
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;

        public CustomDataSetController(AppState appState, IRegionManager rm, IEventAggregator ea)
        {
            _appState = appState;
            _rm = rm;
            _ea = ea;

            PlotMouseDownCommand = new DelegateCommand<OxyMouseDownEventArgs>(PlotMouseDown);
            OpenDivisionViewCommand = new DelegateCommand(OpenDivisionView,
                () => _appState.ActiveSession.TrainingData != null);
            SelectVariablesCommand = new DelegateCommand(SelectVariables,
                () => _appState.ActiveSession.TrainingData != null);

            CustomDataSetViewModel.Created += () =>
            {
                //create session when navigated
                _appState.CreateSession();

                //TODO
                //_actionMenuNavService.SetLeftMenu<ActionMenuLeftView>();
            };
        }

        public DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        public DelegateCommand OpenDivisionViewCommand { get; set; }
        public DelegateCommand SelectVariablesCommand { get; set; }

        private void SelectVariables()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Select variables"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate(nameof(VariablesSelectionViewModel));
        }

        private void OpenDivisionView()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Divide data set"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate(nameof(DataSetDivisionViewModel),
                new InMemoryDataSetDivisionNavParams(_input, _target));
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
            _input.Add(new[] {p.X});
            _target.Add(new[] {p.Y});

            SortPoints();

            if (_input.Count >= 3)
            {
                var sets = new SupervisedTrainingSets(SupervisedSet.FromArrays(_input.ToArray(),
                    _target.ToArray()));

                var trainingData = new TrainingData(sets,
                    new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] {0}, new[] {1}),
                        new[] {new VariableName("x"), new VariableName("y"),}), TrainingDataSource.Memory);

                _appState.ActiveSession.TrainingData = trainingData;

                OpenDivisionViewCommand.RaiseCanExecuteChanged();
                SelectVariablesCommand.RaiseCanExecuteChanged();
            }
        }
    }
}