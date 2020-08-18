using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Data.Presentation.Services;
using FluentAssertions;
using Infrastructure;
using Infrastructure.Domain;
using Moq;
using Moq.AutoMock;
using Prism.Commands;
using TestUtils;
using Xunit;

namespace Data.Tests.Application.DataSourceSelection
{
    public class SingleFileSourceTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private Mock<ICsvValidationService> _csvValidation;
        private SingleFileService _singleFileService;
        private Mock<ISupervisedDataSetService> _dataSetService;

        private SingleFileSourceController _ctrl;
        private SingleFileSourceViewModel _vm;
        private AppState _appState;

        public SingleFileSourceTests()
        {
            _mocker.UseTestRm();
            _csvValidation = _mocker.UseMock<ICsvValidationService>();
            _dataSetService = _mocker.UseMock<ISupervisedDataSetService>();
            _appState = _mocker.UseMock<AppState>().Object;
            _ctrl = _mocker.UseImpl<ITransientControllerBase<SingleFileService>, SingleFileSourceController>();
            _singleFileService = _mocker.UseImpl<ISingleFileService, SingleFileService>();

            _vm = _mocker.CreateInstance<SingleFileSourceViewModel>();
        }

        [Fact]
        public async void Validation_success_scenario()
        {
            //arrange
            var singleFileService = _singleFileService;

            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));

            //act & assert
            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeFalse();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();

            //stop load exec
            var prev = singleFileService.LoadCommand;
            singleFileService.LoadCommand = new DelegateCommand<string>(_ => { }, _ => false);

            singleFileService.ValidateCommand.Execute("file.csv");
            //wait for async call completion
            await Task.Delay(500);

            singleFileService.LoadCommand = prev;

            //file is valid
            singleFileService.FileValidationResult.IsFileValid.Should().BeTrue();
            //can run validate cmd
            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            //can run load cmd
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeTrue();
            //can return
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();
            //cannot continue
            singleFileService.ContinueCommand.CanExecute().Should().BeFalse();
        }

        [Fact]
        public async void Validation_fail_scenario()
        {
            //arrange
            var singleFileService = _singleFileService;
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((false, "error", 2, 2));

            //act & assert
            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeFalse();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();

            singleFileService.ValidateCommand.Execute("file.csv");
            //wait for async call completion
            await Task.Delay(500);

            singleFileService.FileValidationResult.IsFileValid.Should().BeFalse();

            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeFalse();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();
        }


        [Fact]
        public async void Continue_command_sets_app_state()
        {
            //arrange
            var trainingData = TrainingDataMocks.ValidData1;
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _dataSetService.Setup(f => f.LoadDefaultSet(It.IsAny<string>())).Returns(trainingData);
            var singleFileService = _singleFileService;


            //act & assert
            singleFileService.ContinueCommand.CanExecute().Should().BeFalse();

            singleFileService.ValidateCommand.Execute("file.csv");

            //wait for async call completion
            await Task.Delay(2_000);

            //variables are set
            _vm.Variables.Length.Should().BeGreaterThan(0);
            singleFileService.ContinueCommand.CanExecute().Should().BeTrue();

            singleFileService.ContinueCommand.Execute();

            _appState.SessionManager.Sessions.Count.Should().Be(1);
            var session = _appState.SessionManager.ActiveSession;

            session.TrainingData.Should().Be(trainingData);
        }
    }
}