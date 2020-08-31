using System;
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
using System.Diagnostics;
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
    internal class CustomDataSetMemento
    {
        private Dictionary<Session, (IReadOnlyList<double[]> input, IReadOnlyList<double[]> target)> _storage = new Dictionary<Session, (IReadOnlyList<double[]> input, IReadOnlyList<double[]> target)>();

        public void SaveForSession(CustomDataSetController ctrl, Session session)
        {
            _storage[session] = (ctrl.Input.Select(v => v).ToList(), ctrl.Target.Select(v => v).ToList());
        }

        public bool TryRestoreForSession(CustomDataSetController ctrl, Session session)
        {
            if (_storage.TryGetValue(session, out var data))
            {
                ctrl.Input = data.input;
                ctrl.Target = data.target;
                return true;
            }

            return false;
        }
    }

    internal class CustomDataSetController : ICustomDataSetService, ITransientController
    {
        private readonly AppState _appState;
        private List<double[]> _input = new List<double[]>();
        private List<double[]> _target = new List<double[]>();
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;
        private readonly IViewModelAccessor _accessor;
        private readonly CustomDataSetMemento _memento;

        public CustomDataSetController(AppState appState, IRegionManager rm, IEventAggregator ea, IViewModelAccessor accessor, CustomDataSetMemento memento)
        {
            _appState = appState;
            _rm = rm;
            _ea = ea;
            _accessor = accessor;
            _memento = memento;

            PlotMouseDownCommand = new DelegateCommand<OxyMouseDownEventArgs>(PlotMouseDown);
            OpenDivisionViewCommand = new DelegateCommand(OpenDivisionView,
                () => _appState.ActiveSession!.TrainingData != null);
            SelectVariablesCommand = new DelegateCommand(SelectVariables,
                () => _appState.ActiveSession!.TrainingData != null);

            _accessor.OnCreated<CustomDataSetViewModel>(() =>
            {
                if(_appState.ActiveSession != null) AppStateOnActiveSessionChanged(this, (null, _appState.ActiveSession));

                _appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;
            });
        }

        ///memento getters / setters
        public IReadOnlyList<double[]> Input
        {
            get => _input;
            set
            {
                _input = value.Select(v => v).ToList();
                TryRestoreVmPoints();
            }
        }

        public IReadOnlyList<double[]> Target
        {
            get => _target;
            set
            {
                _target = value.Select(v => v).ToList();
                TryRestoreVmPoints();
            }
        }

        public DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        public DelegateCommand OpenDivisionViewCommand { get; set; }
        public DelegateCommand SelectVariablesCommand { get; set; }


        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) args)
        {
            if (args.prev != null)
            {
                var vm = _accessor.Get<CustomDataSetViewModel>()!;

                vm.Scatter.Points.Clear();
                vm.Line.Points.Clear();

                _memento.SaveForSession(this, args.prev);
            }
            
            if (args.next.TrainingData != null && args.next.TrainingData.Source != TrainingDataSource.Memory)
            {
                _appState.ActiveSessionChanged -= AppStateOnActiveSessionChanged;
                return;
            }

            _input.Clear();
            _target.Clear();

            _memento.TryRestoreForSession(this, args.next);
        }

        private void TryRestoreVmPoints()
        {
            if (_input.Count == _target.Count)
            {
                var vm = _accessor.Get<CustomDataSetViewModel>()!;

                for (int i = 0; i < _input.Count; i++)
                {
                    vm.Scatter.Points.Add(new ScatterPoint(_input[i][0], _target[i][0]));
                }

                var sortedPoints = vm.Scatter.Points.Select(p => new DataPoint(p.X, p.Y)).OrderBy(p => p.X);
                vm.Line.Points.AddRange(sortedPoints);
                vm.PlotModel.InvalidatePlot(false);
            }
        }

        private void SelectVariables()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Select variables"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate("VariablesSelectionView");
        }

        private void OpenDivisionView()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Divide data set"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate("DataSetDivisionView",
                new InMemoryDataSetDivisionNavParams(_input, _target));
        }

        private void PlotMouseDown(OxyMouseDownEventArgs args)
        {
            if (args.ChangedButton == OxyMouseButton.Left && args.ClickCount == 2)
            {
                var vm = CustomDataSetViewModel.Instance!;

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

                _appState.ActiveSession!.TrainingData = trainingData;

                OpenDivisionViewCommand.RaiseCanExecuteChanged();
                SelectVariablesCommand.RaiseCanExecuteChanged();
            }
        }
    }
}