using Common.Domain;
using Common.Framework;
using Data.Application.Controllers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace Data.Application.ViewModels
{
    public class MultiFileSourceViewModel : ViewModelBase<MultiFileSourceViewModel>
    {
        private string? _trainingSetFilePath;
        private string? _validationSetFilePath;
        private string? _testSetFilePath;
        private string? _testSetFileName;
        private string? _trainingSetFileName;
        private string? _validationSetFileName;
        private int? _totalRows;
        private string? _ratio;
        private VariablesTableModel[]? _variables;

        public MultiFileSourceViewModel(IMultiFileSourceController multiFileService)
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

            multiFileService.Initialize(this);
        }

        private void AttachValidationResultChangeHanlder(FileValidationResult? result)
        {
            if(result == null) return;
            
            decimal calcPrerc(FileValidationResult res)
            {
                return res.Rows * 100.0m / (TotalRows.GetValueOrDefault() == 0 ? 1 : TotalRows.GetValueOrDefault());
            }

            result.PropertyChanged += (sender, args) =>
            {
                switch (args.PropertyName)
                {
                    case nameof(FileValidationResult.IsFileValid):
                        if (MultiFileValidationResult.All(r => r.IsFileValid == true) || 
                            MultiFileValidationResult[0].IsFileValid.GetValueOrDefault() && 
                            MultiFileValidationResult[1].IsFileValid.GetValueOrDefault() && 
                            TestSetFilePath == null || MultiFileValidationResult[0].IsFileValid.GetValueOrDefault() &&
                            MultiFileValidationResult[2].IsFileValid.GetValueOrDefault() &&
                            ValidationSetFilePath == null)
                        {
                            MultiFileService.LoadFiles.Execute((TrainingSetFilePath!, ValidationSetFilePath!,
                                TestSetFilePath!));
                        }

                        break;
                    case nameof(FileValidationResult.IsLoaded):
                        if (MultiFileValidationResult.All(r => r.IsLoaded))
                        {
                            TotalRows = MultiFileValidationResult.Sum(r => r.Rows);

                            var t = calcPrerc(MultiFileValidationResult[0]);
                            var v = calcPrerc(MultiFileValidationResult[1]);
                            var ts = calcPrerc(MultiFileValidationResult[2]);
                            var tperc = t % 1 == 0 ? t.ToString("F0") : Math.Round(t, 2).ToString();
                            var vperc = v % 1 == 0 ? v.ToString("F0") : Math.Round(v, 2).ToString();
                            var tsperc = ts % 1 == 0 ? ts.ToString("F0") : Math.Round(ts, 2).ToString();

                            Ratio = $"{tperc}%:{vperc}%:{tsperc}%";
                        }
                        break;
                }
            };
        }

        public IMultiFileSourceController MultiFileService { get; }

        public ObservableCollection<FileValidationResult> MultiFileValidationResult { get; set; }

        public VariablesTableModel[]? Variables
        {
            get => _variables;
            set => SetProperty(ref _variables, value);
        }

        public int? TotalRows
        {
            get => _totalRows;
            set => SetProperty(ref _totalRows, value);
        }

        public string? Ratio
        {
            get => _ratio;
            set => SetProperty(ref _ratio, value);
        }

        public string? TrainingSetFilePath
        {
            get => _trainingSetFilePath;
            set
            {
                SetProperty(ref _trainingSetFilePath, value);
                MultiFileValidationResult[0] = new FileValidationResult();
                Variables = null;

                if (value != null)
                {
                    TrainingSetFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                    MultiFileService.ValidateTrainingFile.Execute(value);
                }
            }
        }

        public string? ValidationSetFilePath
        {
            get => _validationSetFilePath;
            set
            {
                SetProperty(ref _validationSetFilePath, value);
                MultiFileValidationResult[1] = new FileValidationResult();
                Variables = null;

                if (value != null)
                {
                    ValidationSetFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                    MultiFileService.ValidateValidationFile.Execute(value);
                }
            }
        }

        public string? TestSetFilePath
        {
            get => _testSetFilePath;
            set
            {
                SetProperty(ref _testSetFilePath, value);
                MultiFileValidationResult[2] = new FileValidationResult();
                Variables = null;
                if(value != null)
                {
                    TestSetFileName = value.Split('\\', StringSplitOptions.RemoveEmptyEntries)[^1];
                    MultiFileService.ValidateTestFile.Execute(value);
                }
            }
        }

        public string? TestSetFileName
        {
            get => _testSetFileName;
            set => SetProperty(ref _testSetFileName, value);
        }

        public string? TrainingSetFileName
        {
            get => _trainingSetFileName;
            set => SetProperty(ref _trainingSetFileName, value);
        }

        public string? ValidationSetFileName
        {
            get => _validationSetFileName;
            set => SetProperty(ref _validationSetFileName, value);
        }


        public FileValidationResult TrainingValidationResult => MultiFileSourceViewModel.Instance!.MultiFileValidationResult[0];
        public FileValidationResult ValidationValidationResult => MultiFileSourceViewModel.Instance!.MultiFileValidationResult[1];
        public FileValidationResult TestValidationResult => MultiFileSourceViewModel.Instance!.MultiFileValidationResult[2];

        public void SetTrainingValidationResult(bool isValid, string? error = null, bool hasContentError = false,
           int r = 0, int c = 0)
        {
            TrainingValidationResult.Rows = r;
            TrainingValidationResult.Cols = c;
            TrainingValidationResult.IsValidatingFile = false;
            TrainingValidationResult.IsFileValid = isValid;
            TrainingValidationResult.HasContentError = hasContentError;
            TrainingValidationResult.FileValidationError = error;
        }

        public void SetTrainingValidating(bool isValidating) =>
            TrainingValidationResult.IsValidatingFile = isValidating;

        public void SetValidationValidationResult(bool isValid, string? error = null, bool hasContentError = false,
            int r = 0, int c = 0)
        {
            ValidationValidationResult.Rows = r;
            ValidationValidationResult.Cols = c;
            ValidationValidationResult.IsValidatingFile = false;
            ValidationValidationResult.IsFileValid = isValid;
            ValidationValidationResult.HasContentError = hasContentError;
            ValidationValidationResult.FileValidationError = error;
        }

        public void SetValidationValidating(bool isValidating) =>
            ValidationValidationResult.IsValidatingFile = isValidating;


        public void SetTestValidationResult(bool isValid, string? error = null, bool hasContentError = false, int r = 0,
            int c = 0)
        {
            TestValidationResult.Rows = r;
            TestValidationResult.Cols = c;
            TestValidationResult.IsValidatingFile = false;
            TestValidationResult.IsFileValid = isValid;
            TestValidationResult.HasContentError = hasContentError;
            TestValidationResult.FileValidationError = error;
        }

        public void SetTestValidating(bool isValidating) =>
            TestValidationResult.IsValidatingFile = isValidating;


        public void SetIsLoading() => TrainingValidationResult.IsLoadingFile =
            ValidationValidationResult.IsLoadingFile = TestValidationResult.IsLoadingFile = true;

        public void SetLoaded(TrainingData trainingData)
        {
            TrainingValidationResult.IsLoadingFile =
                ValidationValidationResult.IsLoadingFile = TestValidationResult.IsLoadingFile = false;
            TrainingValidationResult.IsLoaded =
                ValidationValidationResult.IsLoaded = TestValidationResult.IsLoaded = true;
            MultiFileSourceViewModel.Instance!.Variables = trainingData.Variables.InputVariableNames.Union(trainingData.Variables.TargetVariableNames)
                .Select((s, i) => new VariablesTableModel()
                {
                    Column = i + 1,
                    Name = s
                }).ToArray();
        }
    }
}