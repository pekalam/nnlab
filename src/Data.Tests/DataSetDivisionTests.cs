using Common.Domain;
using Data.Application.Controllers;
using Data.Application.ViewModels;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Regions;
using System;
using System.Collections.Generic;
using TestUtils;
using Xunit;

namespace Data.Application.Tests
{
    public class DataSetDivisionTests
    {
        private AutoMocker _mocker = new AutoMocker();

        private DataSetDivisionController _service;
        private DataSetDivisionViewModel _vm;
        private AppState _appState;

        public DataSetDivisionTests()
        {
            _mocker.UseTestEa();
            _mocker.UseTestRm();
            _appState = _mocker.UseImpl<AppState>();

        }

        [Fact]
        public void DivideMemoryDataCommand_sets_divided_training_data_in_appstate()
        {
            //arrange
            var session = _appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidData2;
            _service = _mocker.UseImpl<IDataSetDivisionController, DataSetDivisionController>();
            _vm = _mocker.UseVm<DataSetDivisionViewModel>();
            _vm.TrainingSetPercent = 50;
            _vm.TestSetPercent = _vm.ValidationSetPercent = 25;

            var input = new List<double[]>(new []{ new[] { 0d }, new[] { 1d }, new[] { 2d }, new[] { 3d } });
            var target = new List<double[]>(new[] { new[] { 0d }, new[] { 1d }, new[] { 2d }, new[] { 3d } });

            //act
            _service.DivideMemoryDataCommand.Execute((input, target));


            //assert
            session.TrainingData.Sets.ValidationSet.Input.Count.Should().Be(1);
            session.TrainingData.Sets.TestSet.Input.Count.Should().Be(1);
            session.TrainingData.Sets.TrainingSet.Input.Count.Should().Be(2);
            
            session.TrainingData.OriginalSets.ValidationSet.Input.Count.Should().Be(1);
            session.TrainingData.OriginalSets.TestSet.Input.Count.Should().Be(1);
            session.TrainingData.OriginalSets.TrainingSet.Input.Count.Should().Be(2);
        }

        [Fact]
        public void DataSetDivisionVm_when_navigated_sets_cmd_params_based_on_nav_params()
        {
            //arrange
            var input = new List<double[]>(new[] { new[] { 0d }, new[] { 1d }, new[] { 2d }, new[] { 3d } });
            var target = new List<double[]>(new[] { new[] { 0d }, new[] { 1d }, new[] { 2d }, new[] { 3d } });
            var ctx = new NavigationContext(Mock.Of<IRegionNavigationService>(service => service.Region == new Region()),new Uri("ViewTEST", UriKind.Relative), new InMemoryDataSetDivisionNavParams(input, target));
            _service = _mocker.UseImpl<IDataSetDivisionController, DataSetDivisionController>();
            _vm = _mocker.UseVm<DataSetDivisionViewModel>();
            //act
            _vm.OnNavigatedTo(ctx);

            //assert
            var memCmdParam = (((List<double[]> input, List<double[]> target)?) _vm.DivideCommandParam).Value;
            memCmdParam.input.Should().BeEquivalentTo(input);
            memCmdParam.target.Should().BeEquivalentTo(target);
            _vm.DivideCommand.Should().Be(_service.DivideMemoryDataCommand);

            //arrange
            string filePath = "path";
            ctx = new NavigationContext(Mock.Of<IRegionNavigationService>(service => service.Region == new Region()), new Uri("ViewTEST", UriKind.Relative), new FileDataSetDivisionNavParams(filePath));
            
            //act
            _vm.OnNavigatedTo(ctx);

            //assert
            _vm.DivideCommandParam.Should().BeEquivalentTo(filePath);
            _vm.DivideCommand.Should().Be(_service.DivideFileDataCommand);
        }

        [Fact]
        public void DataSetDivisionVm_when_invalid_percent_params_cmd_cannot_exec()
        {
            _service = _mocker.UseImpl<IDataSetDivisionController, DataSetDivisionController>();
            _vm = _mocker.UseVm<DataSetDivisionViewModel>();
            _service.DivideMemoryDataCommand.CanExecute(null).Should().BeFalse();
            _service.DivideFileDataCommand.CanExecute(null).Should().BeFalse();
            _vm[nameof(DataSetDivisionViewModel.TrainingSetPercent)].Should().BeNullOrEmpty();

            _vm.TestSetPercent = _vm.ValidationSetPercent = 25;
            _vm[nameof(DataSetDivisionViewModel.TrainingSetPercent)].Should().NotBeNullOrEmpty();
            _service.DivideMemoryDataCommand.CanExecute(null).Should().BeFalse();
            _service.DivideFileDataCommand.CanExecute(null).Should().BeFalse();

            _vm.TrainingSetPercent = 50;
            _vm[nameof(DataSetDivisionViewModel.TrainingSetPercent)].Should().BeNullOrEmpty();
            _service.DivideMemoryDataCommand.CanExecute(null).Should().BeTrue();
            _service.DivideFileDataCommand.CanExecute(null).Should().BeTrue();
        }


        [Fact]
        public void DataSetDivisionVm_sets_data_set_percents_based_on_app_state()
        {
            var session = _appState.CreateSession();
            session.TrainingData = TrainingDataMocks.ValidData1;
            _service = _mocker.UseImpl<IDataSetDivisionController, DataSetDivisionController>();
            _vm = _mocker.UseVm<DataSetDivisionViewModel>();

            _vm.TrainingSetPercent.Should().Be(0);
            _vm.ValidationSetPercent.Should().Be(0);
            _vm.TestSetPercent.Should().Be(0);

            _vm.Ratio.Should().Be("100%:0%:0%");
        }
    }
}
