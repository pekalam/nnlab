#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
using Common.Framework;
using Prism.Commands;
using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;
using Common.Domain;
using Data.Application.ViewModels;

namespace Data.Application.Services
{
    public interface IMultiFileService : IService, ITransientController
    {
        DelegateCommand SelectTrainingFileCommand { get; set; }
        DelegateCommand SelectValidationFileCommand { get; set; }
        DelegateCommand SelectTestFileCommand { get; set; }

        DelegateCommand ReturnCommand { get; set; }
        DelegateCommand ContinueCommand { get; set; }

        DelegateCommand<string> ValidateTrainingFile { get; set; }
        DelegateCommand<string> ValidateTestFile { get; set; }
        DelegateCommand<string> ValidateValidationFile { get; set; }

        DelegateCommand<(string trainingFile, string validationFile, string testFile)?> LoadFiles { get; set; }
    }
}