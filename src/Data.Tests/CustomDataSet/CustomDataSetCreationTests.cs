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
            _appState = _mocker.UseImpl<AppState>();
            _ctrl = _mocker.UseImpl<ICustomDataSetService,CustomDataSetController>();
            _vm = _mocker.CreateInstance<CustomDataSetViewModel>();

            _dsService = _ctrl;
        }



        [Fact]
        public void New_session_is_created_after_viewmodel_is_created()
        {
            _appState.Sessions.Count.Should().Be(1);
            _appState.ActiveSession.Should().NotBeNull();
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
    }
}
