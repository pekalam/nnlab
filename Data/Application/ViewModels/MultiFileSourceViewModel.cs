using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Data.Application.Services;
using Infrastructure;

namespace Data.Application.ViewModels
{
    public class MultiFileSourceViewModel : ViewModelBase<MultiFileSourceViewModel>
    {
        private string _trainingSetFilePath;
        private string _validationSetFilePath;
        private string _testSetFilePath;
        private string _testSetFileName;
        private string _trainingSetFileName;
        private string _validationSetFileName;
        private int? _totalRows;
        private int _trainingRatio;
        private string _ratio;

        public MultiFileSourceViewModel(IMultiFileService multiFileService)
        {
            MultiFileService = multiFileService;
            KeepAlive = false;

            multiFileService.MultiFileValidationResult.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Replace)
                {
                    TotalRows = null;
                    Ratio = null;
                    AttachValidationResultChangeHanlder(args.NewItems[0] as FileValidationResult);
                }
            };
            foreach (var result in multiFileService.MultiFileValidationResult)
            {
                AttachValidationResultChangeHanlder(result);
            }
        }

        private void AttachValidationResultChangeHanlder(FileValidationResult result)
        {
            result.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(FileValidationResult.IsFileValid):
                        if (!MultiFileService.MultiFileValidationResult.Any(result => result.IsFileValid != true))
                        {
                            MultiFileService.LoadFiles.Execute((TrainingSetFilePath, ValidationSetFilePath,
                                TestSetFilePath));
                        }

                        break;
                    case nameof(FileValidationResult.IsLoaded):
                        if (MultiFileService.MultiFileValidationResult.All(r => r.IsLoaded))
                        {
                            TotalRows = MultiFileService.MultiFileValidationResult.Sum(r => r.Rows);
                            Ratio =
                                $"{MultiFileService.MultiFileValidationResult[0].Rows * 100 / (TotalRows == 0 ? 1 : TotalRows):D}:{MultiFileService.MultiFileValidationResult[1].Rows * 100 / (TotalRows == 0 ? 1 : TotalRows):D}:{MultiFileService.MultiFileValidationResult[2].Rows * 100 / (TotalRows == 0 ? 1 : TotalRows):D}";
                        }
                        break;
                }
            };
        }


        public IMultiFileService MultiFileService { get; set; }

        public int? TotalRows
        {
            get => _totalRows;
            set => SetProperty(ref _totalRows, value);
        }

        public string Ratio
        {
            get => _ratio;
            set => SetProperty(ref _ratio, value);
        }

        public string TrainingSetFilePath
        {
            get => _trainingSetFilePath;
            set
            {
                SetProperty(ref _trainingSetFilePath, value);
                TrainingSetFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                MultiFileService.ValidateTrainingFile.Execute(value);
            }
        }

        public string ValidationSetFilePath
        {
            get => _validationSetFilePath;
            set
            {
                SetProperty(ref _validationSetFilePath, value);
                ValidationSetFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                MultiFileService.ValidateValidationFile.Execute(value);
            }
        }

        public string TestSetFilePath
        {
            get => _testSetFilePath;
            set
            {
                SetProperty(ref _testSetFilePath, value);
                TestSetFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                MultiFileService.ValidateTestFile.Execute(value);
            }
        }

        public string TestSetFileName
        {
            get => _testSetFileName;
            set => SetProperty(ref _testSetFileName, value);
        }

        public string TrainingSetFileName
        {
            get => _trainingSetFileName;
            set => SetProperty(ref _trainingSetFileName, value);
        }

        public string ValidationSetFileName
        {
            get => _validationSetFileName;
            set => SetProperty(ref _validationSetFileName, value);
        }
    }
}