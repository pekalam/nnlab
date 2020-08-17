using System;
using System.Collections.ObjectModel;
using Prism.Commands;

namespace Data.Application.Services
{
    public interface IMultiFileService
    {
        DelegateCommand SelectTrainingFileCommand { get; set; }
        DelegateCommand SelectValidationFileCommand { get; set; }
        DelegateCommand SelectTestFileCommand { get; set; }

        DelegateCommand ReturnCommand { get; set; }
        DelegateCommand ContinueCommand { get; set; }

        DelegateCommand ValidateTrainingFile { get; set; }
        DelegateCommand ValidateTestFile { get; set; }
        DelegateCommand ValidateValidationFile { get; set; }

        DelegateCommand LoadTrainingFile { get; set; }
        DelegateCommand LoadTestFile { get; set; }
        DelegateCommand LoadValidationFile { get; set; }

        FileValidationResult[] MultiFileValidationResult { get; set; }
    }

    public class MultiFileService : IMultiFileService
    {
        public MultiFileService()
        {
            Array.Fill(MultiFileValidationResult, new FileValidationResult());
        }

        public DelegateCommand SelectTrainingFileCommand { get; set; }
        public DelegateCommand SelectValidationFileCommand { get; set; }
        public DelegateCommand SelectTestFileCommand { get; set; }
        public DelegateCommand ReturnCommand { get; set; }
        public DelegateCommand ContinueCommand { get; set; }
        public DelegateCommand ValidateTrainingFile { get; set; }
        public DelegateCommand ValidateTestFile { get; set; }
        public DelegateCommand ValidateValidationFile { get; set; }
        public DelegateCommand LoadTrainingFile { get; set; }
        public DelegateCommand LoadTestFile { get; set; }
        public DelegateCommand LoadValidationFile { get; set; }
        public FileValidationResult[] MultiFileValidationResult { get; set; } = new FileValidationResult[3];


        public void Reset()
        {
            MultiFileValidationResult = new FileValidationResult[3];
            Array.Fill(MultiFileValidationResult, new FileValidationResult());
        }
    }
}