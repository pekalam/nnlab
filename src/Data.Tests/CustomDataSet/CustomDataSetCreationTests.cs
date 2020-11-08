using System;
using System.Linq;
using Common.Domain;
using Data.Application.Controllers;

using Data.Application.ViewModels;
using FluentAssertions;
using MahApps.Metro.IconPacks.Converter;
using Moq.AutoMock;
using OxyPlot;
using SharedUI.MatrixPreview;
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
        private ICustomDataSetController _dsController;

        public CustomDataSetCreationTests()
        {
            _mocker.UseTestRm();
            _mocker.UseTestEa();
            _appState = _mocker.UseImpl<AppState>();
            _appState.CreateSession();

            var matVm = _mocker.UseImpl<MatrixPreviewViewModel>();
            matVm.UpdateColumns = _ => { };
            _ctrl = _mocker.UseImpl<ICustomDataSetController,CustomDataSetController>();
            _vm = _mocker.UseVm<CustomDataSetViewModel>();
            _vm.OnNavigatedTo(null);

            _dsController = _ctrl;

        }

        private void AddPoint(double x, double y)
        {
            _dsController.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2,
                Position = new ScreenPoint(x, y),
                ChangedButton = OxyMouseButton.Left,
            });
        }

        [Fact]
        public void Training_data_is_created_with_3_points_when_vm_is_created()
        {
            _appState.ActiveSession.TrainingData.Should().NotBeNull();
            _appState.ActiveSession.TrainingData.Sets.TrainingSet.Input.Count.Should().Be(3);
            _appState.ActiveSession.TrainingData.Sets.TrainingSet.Target.Count.Should().Be(3);
        }

        [Fact]
        public void MouseDownCommand_updates_session_training_data()
        {
            //act
            _dsController.PlotMouseDownCommand.Execute(new OxyMouseDownEventArgs()
            {
                ClickCount = 2, Position = new ScreenPoint(0,0), ChangedButton = OxyMouseButton.Left,
            });

            _appState.ActiveSession.TrainingData.Sets.TrainingSet.Input.Count.Should().Be(4);
            _appState.ActiveSession.TrainingData.Sets.TrainingSet.Target.Count.Should().Be(4);


            _vm.MatrixVm.Controller.AssignedMatrix.RowCount.Should().Be(4);
        }

    }
}
