using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Data.Application.Controllers;
using Data.Application.Services;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Data.Presentation.Services;
using Data.Presentation.Views;
using FluentAssertions;
using Infrastructure;
using Moq;
using Moq.AutoMock;
using Prism.Commands;
using Prism.Regions;
using TestUtils;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Data.Tests.Application.DataSourceSelection
{
    public class FileSelectTest
    {
        private readonly AutoMocker _mocker = new AutoMocker();
        private Mock<SingleFileService> _singleFileService;
        private Mock<MultiFileService> _multiFileService;
        private Mock<FileService> _fileService;
        private Mock<IFileDialogService> _dialogService;
        private Mock<IRegionManager> _rm;
        private Dictionary<string, Mock<IRegion>> _regions;

        private FileController _ctrl;

        public FileSelectTest()
        {
            (_rm, _regions) = _mocker.UseTestRm();
            _fileService = _mocker.UseInMocker<IFileService, FileService>();
            _singleFileService = _mocker.UseInMocker<ISingleFileService, SingleFileService>();
            _multiFileService = _mocker.UseInMocker<IMultiFileService, MultiFileService>();
            _dialogService = _mocker.UseInMocker<IFileDialogService>();

            _ctrl= _mocker.CreateInstance<FileController>();
        }

        [Fact]
        public void SelectFileCommand_when_dialog_returns_path_navigates_to_next_view()
        {
            //arrange
            var vm = _mocker.CreateInstance<SelectDataSourceViewModel>(); //current vm
            var sigVm = _mocker.CreateInstance<SingleFileSourceViewModel>(); //next view vm

            var successfulPath = "C:\\Dir\\test.csv";

            //stop validation execution
            _singleFileService.Object.ValidateCommand = new DelegateCommand<string>(s => { }, _ => false);
            //setup dialog success
            _dialogService.Setup(f => f.OpenCsv(It.IsAny<Window>())).Returns((true, successfulPath));

            //act
            vm.FileService.SelectFileCommand.Execute();


            //assert
            _regions[AppRegions.ContentRegion].VerifyNavigation(nameof(SingleFileSourceView), Times.Exactly(1));


            sigVm.SelectedFilePath.Should().Be(successfulPath);
            sigVm.SelectedFileName.Should().Be("test.csv");
        }


        [Fact]
        public void SelectFileCommand_when_dialog_returns_empty_path_doesnt_navigate()
        {
            //arrange
            var vm = _mocker.CreateInstance<SelectDataSourceViewModel>();
            var sigVm = _mocker.CreateInstance<SingleFileSourceViewModel>();


            _dialogService.Setup(f => f.OpenCsv(It.IsAny<Window>())).Returns((false, default));

            //act
            vm.FileService.SelectFileCommand.Execute();


            //assert
            _regions[AppRegions.ContentRegion].VerifyNavigation(nameof(SingleFileSourceView), Times.Never());


            sigVm.SelectedFilePath.Should().Be(default);
            sigVm.SelectedFileName.Should().Be(default);
        }



        [Fact]
        public void SelectTSVFiles_when_dialog_returns_valid_path_sets_paths()
        {
            //arrange
            var vm = _mocker.CreateInstance<SelectDataSourceViewModel>();
            var mulVm = _mocker.CreateInstance<MultiFileSourceViewModel>();

            var trainingPath = "C:\\Dir\\training.csv";
            var validationPath = "C:\\Dir\\validation.csv";
            var testPath = "C:\\Dir\\test.csv";

            //act & assert
            //training file
            _dialogService.Setup(f => f.OpenCsv(It.IsAny<Window>())).Returns((true, trainingPath));
            _multiFileService.Object.SelectTrainingFileCommand.Execute();
            mulVm.TrainingSetFilePath.Should().Be(trainingPath);

            //validation file
            _dialogService.Setup(f => f.OpenCsv(It.IsAny<Window>())).Returns((true, validationPath));
            _multiFileService.Object.SelectValidationFileCommand.Execute();
            mulVm.ValidationSetFilePath.Should().Be(validationPath);

            //training file
            _dialogService.Setup(f => f.OpenCsv(It.IsAny<Window>())).Returns((true, testPath));
            _multiFileService.Object.SelectTestFileCommand.Execute();
            mulVm.TestSetFilePath.Should().Be(testPath);
        }
    }
}