using System;
using System.Linq;
using Common.Domain;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels.CustomDataSet;
using FluentAssertions;
using Moq.AutoMock;
using OxyPlot;
using TestUtils;
using Xunit;

namespace Data.Application.Tests.CustomDataSet
{
    public class CustomDataSetCreationTests
    {
        private AutoMocker _mocker = new AutoMocker();

        private CustomDataSetController _ctrl;
        private CustomDataSetViewModel _vm;
        private AppState _appState;
        private ICustomDataSetService _dsService;

        public CustomDataSetCreationTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _mocker.UseTestVmAccessor();
            _appState = _mocker.UseImpl<AppState>();
            _appState.CreateSession();

            _ctrl = _mocker.UseImpl<ICustomDataSetService,CustomDataSetController>();
            _vm = _mocker.UseVm<CustomDataSetViewModel>();


            _dsService = _ctrl;

        }

        private void AddPoint(double x, double y)
        {
            _dsService.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2,
                Position = new ScreenPoint(x, y),
                ChangedButton = OxyMouseButton.Left,
            });
        }

        [Fact]
        public void MouseDownCommand_updates_session_training_data_when_3_points_are_created()
        {
            //act
            _dsService.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2, Position = new ScreenPoint(0,0), ChangedButton = OxyMouseButton.Left,
            });
            //assert
            _appState.ActiveSession.TrainingData.Should().BeNull();

            //act
            _dsService.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2, Position = new ScreenPoint(1, 0), ChangedButton = OxyMouseButton.Left,
            });
            //assert
            _appState.ActiveSession.TrainingData.Should().BeNull();


            //act
            _dsService.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2, Position = new ScreenPoint(2, 0), ChangedButton = OxyMouseButton.Left,
            });
            //assert
            _appState.ActiveSession.TrainingData.Should().NotBeNull();

            _appState.ActiveSession.TrainingData.Sets.TrainingSet.Input.Count.Should().Be(3);
            _appState.ActiveSession.TrainingData.Sets.TrainingSet.Target.Count.Should().Be(3);
        }

        [Fact]
        public void VmPoints_when_new_session_created_and_active_are_cleared_and_restored()
        {
            AddPoint(0,1);
            AddPoint(1, 0);


            var previous = _appState.ActiveSession;
            var previousScatter = _vm.Scatter.Points;
            var previousLine = _vm.Line.Points;

            _appState.ActiveSession = _appState.CreateSession();

            _vm.Scatter.Points.Should().BeEmpty();
            _vm.Line.Points.Should().BeEmpty();

            _appState.ActiveSession = previous;

            _vm.Scatter.Points.Should().BeEquivalentTo(previousScatter);
            _vm.Line.Points.Should().BeEquivalentTo(previousLine);
        }

    }
}
