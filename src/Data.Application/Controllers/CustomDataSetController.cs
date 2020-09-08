﻿using System;
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
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using Prism.Ioc;


namespace Data.Application.Services
{
    public interface ICustomDataSetService
    {
        DelegateCommand<OxyMouseDownEventArgs> PlotMouseDownCommand { get; set; }
        DelegateCommand OpenDivisionViewCommand { get; set; }
        DelegateCommand SelectVariablesCommand { get; set; }
        Action<NavigationContext> Navigated { get; }

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

    internal class CustomDataSetController : ControllerBase<CustomDataSetViewModel>,ICustomDataSetService, ITransientController
    {
        private readonly AppState _appState;
        private List<double[]> _input = new List<double[]>();
        private List<double[]> _target = new List<double[]>();
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;
        private readonly CustomDataSetMemento _memento;
        private readonly AppStateHelper _helper;
        private TrainingData _assignedData;
        private Session? _currentSession;

        public CustomDataSetController(AppState appState, IRegionManager rm, IEventAggregator ea, IViewModelAccessor accessor, CustomDataSetMemento memento) : base(accessor)
        {
            _appState = appState;
            _rm = rm;
            _ea = ea;
            _memento = memento;
            _helper = new AppStateHelper(appState);

            PlotMouseDownCommand = new DelegateCommand<OxyMouseDownEventArgs>(PlotMouseDown);
            OpenDivisionViewCommand = new DelegateCommand(OpenDivisionView,
                () => _appState.ActiveSession!.TrainingData != null);
            SelectVariablesCommand = new DelegateCommand(SelectVariables,
                () => _appState.ActiveSession!.TrainingData != null);


            Navigated = _ =>
            {
                if (_appState.ActiveSession!.TrainingData == null)
                {
                    _input.Add(new[] { 0d });
                    _input.Add(new[] { 1d });
                    _input.Add(new[] { 2d });
                    _target.Add(new[] { 0d });
                    _target.Add(new[] { 1d });
                    _target.Add(new[] { 2d });

                    var sets = new SupervisedTrainingSets(SupervisedSet.FromArrays(_input.ToArray(),
                        _target.ToArray()));
                    var trainingData = _assignedData = new TrainingData(sets,
                        new SupervisedSetVariables(new SupervisedSetVariableIndexes(new[] { 0 }, new[] { 1 }),
                            new[] { new VariableName("x"), new VariableName("y"), }), TrainingDataSource.Memory, NormalizationMethod.None);

                    _appState.ActiveSession!.TrainingData = trainingData;
                }

            };
        }

        private void MatrixVmOnMatrixElementChanged(Matrix<double> obj)
        {
            Vm!.Scatter.Points.Clear();
            for (int i = 0; i < obj.RowCount; i++)
            {
                Vm!.Scatter.Points.Add(new ScatterPoint(obj[i, 0], obj[i, 1]));
            }
            Vm!.PlotModel.InvalidatePlot(true);
        }

        private void MatrixVmOnRowRemoved(Matrix<double> obj)
        {
            _input.Clear();
            _target.Clear();

            Vm!.Scatter.Points.Clear();
            for (int i = 0; i < obj.RowCount; i++)
            {
                _input.Add(new []{obj[i,0]});
                _target.Add(new []{obj[i,1]});
                Vm!.Scatter.Points.Add(new ScatterPoint(obj[i,0], obj[i,1]));
            }
            Vm!.PlotModel.InvalidatePlot(true);
        }

        protected override void VmCreated()
        {
            _helper.OnTrainingDataChanged(data =>
            {
                if (_currentSession != null)
                {
                    _memento.SaveForSession(this, _currentSession);
                    _currentSession = null;
                }
                Vm!.MatrixVm.RowRemoved -= MatrixVmOnRowRemoved;
                Vm!.MatrixVm.MatrixElementChanged -= MatrixVmOnMatrixElementChanged;
                if (data.Source != TrainingDataSource.Memory) return;

                _currentSession = _appState.ActiveSession!;

                _input.Clear();
                _target.Clear();

                if (!_memento.TryRestoreForSession(this, _appState.ActiveSession!))
                {
                    Vm!.Scatter.Points.Clear();

                    for (int i = 0; i < data.Sets.TrainingSet.Input.Count; i++)
                    {
                        AddPoint(new DataPoint(data.Sets.TrainingSet.Input[i][0,0], data.Sets.TrainingSet.Target[i][0, 0]));
                    }
                }

                Vm!.MatrixVm.RowRemoved += MatrixVmOnRowRemoved;
                Vm!.MatrixVm.MatrixElementChanged += MatrixVmOnMatrixElementChanged;
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
        public Action<NavigationContext> Navigated { get; }

        private void TryRestoreVmPoints()
        {
            if (_input.Count == _target.Count)
            {
                Vm!.Scatter.Points.Clear();
                var mat = Matrix<double>.Build.Dense(_input.Count, 2);

                for (int i = 0; i < _input.Count; i++)
                {
                    Vm!.Scatter.Points.Add(new ScatterPoint(_input[i][0], _target[i][0]));
                    mat[i, 0] = _input[i][0];
                    mat[i, 1] = _target[i][0];
                }

                Vm!.MatrixVm.Controller.AssignMatrix(mat, new[] { "x", "y" }, i => i.ToString());
                Vm!.PlotModel.InvalidatePlot(true);
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
                var p = Axis.InverseTransform(args.Position, Vm!.PlotModel.Axes[0], Vm!.PlotModel.Axes[1]);

                AddPoint(p);
            }
        }

        private void AddPoint(DataPoint p)
        {
            _input.Add(new[] {p.X});
            _target.Add(new[] {p.Y});

            if (_input.Count >= 3)
            {
                var sets = new SupervisedTrainingSets(SupervisedSet.FromArrays(_input.ToArray(),
                    _target.ToArray()));

                _appState.ActiveSession!.TrainingData!.Sets = sets;
                
                OpenDivisionViewCommand.RaiseCanExecuteChanged();
                SelectVariablesCommand.RaiseCanExecuteChanged();
            }


            Vm!.Scatter.Points.Add(new ScatterPoint(p.X, p.Y));

            //Vm!.Line.Points.Clear();
            // Vm!.Line.Points.AddRange(Vm!.Scatter.Points.Select(p => new DataPoint(p.X, p.Y)));
            Vm!.PlotModel.InvalidatePlot(true);


            var mat = Matrix<double>.Build.Dense(_input.Count, 2);
            for (int i = 0; i < _input.Count; i++)
            {
                mat[i, 0] = _input[i][0];
                mat[i, 1] = _target[i][0];
            }
            Vm!.MatrixVm.Controller.AssignMatrix(mat, new[] { "x", "y" }, i => i.ToString());

        }
    }
}