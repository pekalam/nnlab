using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private string _ratio;
        private VariablesTableModel[] _variables;

        public MultiFileSourceViewModel(IMultiFileService multiFileService)
        {
            MultiFileValidationResult = new ObservableCollection<FileValidationResult>(Enumerable.Repeat(new FileValidationResult(), 3));
            MultiFileService = multiFileService;
            KeepAlive = false;

            MultiFileValidationResult.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Replace)
                {
                    TotalRows = null;
                    Ratio = null;
                    AttachValidationResultChangeHanlder(args.NewItems[0] as FileValidationResult);
                }
            };
            foreach (var result in MultiFileValidationResult)
            {
                AttachValidationResultChangeHanlder(result);
            }
        }

        private void AttachValidationResultChangeHanlder(FileValidationResult result)
        {
            int calcPrerc(FileValidationResult result)
            {
                return (int) Math.Round(result.Rows * 100.0 / (TotalRows.GetValueOrDefault() == 0 ? 1 : TotalRows.GetValueOrDefault()));
            }

            result.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(FileValidationResult.IsFileValid):
                        if (!MultiFileValidationResult.Any(result => result.IsFileValid != true))
                        {
                            MultiFileService.LoadFiles.Execute((TrainingSetFilePath, ValidationSetFilePath,
                                TestSetFilePath));
                        }

                        break;
                    case nameof(FileValidationResult.IsLoaded):
                        if (MultiFileValidationResult.All(r => r.IsLoaded))
                        {
                            TotalRows = MultiFileValidationResult.Sum(r => r.Rows);
                            Ratio =
                                $"{calcPrerc(MultiFileValidationResult[0])}:{calcPrerc(MultiFileValidationResult[1])}:{calcPrerc(MultiFileValidationResult[2])}";
                        }
                        break;
                }
            };
        }

        public IMultiFileService MultiFileService { get; }

        public ObservableCollection<FileValidationResult> MultiFileValidationResult { get; set; }

        public VariablesTableModel[] Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value);
        }

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
                MultiFileValidationResult[0] = new FileValidationResult();
                Variables = null;
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
                MultiFileValidationResult[1] = new FileValidationResult();
                Variables = null;
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
                MultiFileValidationResult[2] = new FileValidationResult();
                Variables = null;
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