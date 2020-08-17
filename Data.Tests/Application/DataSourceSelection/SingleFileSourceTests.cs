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
        private Mock<SingleFileService> _singleFileService;
        private Mock<ISupervisedDataSetService> _dataSetService;

        private SingleFileSourceController _ctrl;
        private SingleFileSourceViewModel _vm;

        public SingleFileSourceTests()
        {
            _csvValidation=_mocker.UseInMocker<ICsvValidationService>();
            _singleFileService = _mocker.UseInMocker<ISingleFileService, SingleFileService>();
            _dataSetService = _mocker.UseInMocker<ISupervisedDataSetService>();
            _mocker.UseTestRm();

            _ctrl = _mocker.CreateInstance<SingleFileSourceController>();
            _vm = _mocker.CreateInstance<SingleFileSourceViewModel>();
        }

        [Fact]
        public async void When_validation_success_command_can_exec_is_changed()
        {
            //arrange
            var singleFileService = _singleFileService.Object;

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

            singleFileService.FileValidationResult.IsFileValid.Should().BeTrue();
            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();
            singleFileService.ContinueCommand.CanExecute().Should().BeFalse();

            //new vm resets controller state
            _vm = _mocker.CreateInstance<SingleFileSourceViewModel>();

            singleFileService.ValidateCommand.CanExecute("file.csv").Should().BeTrue();
            singleFileService.LoadCommand.CanExecute("file.csv").Should().BeFalse();
            singleFileService.ReturnCommand.CanExecute().Should().BeTrue();
            singleFileService.ContinueCommand.CanExecute().Should().BeFalse();

            //file validation result and variables are set to default value
            singleFileService.FileValidationResult.IsFileValid.Should().BeNull();
            singleFileService.Variables.Should().BeNull();
        }

        [Fact]
        public async void When_validation_fails_command_can_exec_is_changed()
        {
            //arrange
            var singleFileService = _singleFileService.Object;
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
        public async void When_loaded_sets_valid_props_and_can_continue()
        {
            //arrange
            _csvValidation.Setup(f => f.Validate(It.IsAny<string>())).Returns((true, null, 2, 2));
            _dataSetService.Setup(f => f.LoadDefaultSet(It.IsAny<string>())).Returns(TrainingDataMocks.ValidData1);
            var singleFileService = _singleFileService.Object;


            //act & assert
            singleFileService.ContinueCommand.CanExecute().Should().BeFalse();

            singleFileService.ValidateCommand.Execute("file.csv");

            //wait for async call completion
            await Task.Delay(2_000);

            singleFileService.ContinueCommand.CanExecute().Should().BeTrue();
            singleFileService.Variables.Length.Should().BeGreaterThan(0);
        }
    }
}