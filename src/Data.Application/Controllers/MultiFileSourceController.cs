using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSourceSelection;
using Data.Domain.Services;
using Infrastructure.Domain;
using Prism.Commands;
using Prism.Regions;
using Shell.Interface;
using System.Linq;

namespace Data.Application.Controllers
{
    internal class MultiFileSourceController : ITransientControllerBase<MultiFileService>
    {
        private MultiFileService _multiFileService;
        private readonly IRegionManager _rm;
        private readonly ICsvValidationService _csvValidationService;
        private readonly ISupervisedDataSetService _dataSetService;
        private readonly IFileDialogService _fileDialogService;
        private readonly AppState _appState;

        private TrainingData _trainingData;

        private string[] _trainingHeaders;
        private string[] _validationHeaders;
        private string[] _testHeaders;

        private bool _loadCanExec;
        private bool _continueCanExec;


        public MultiFileSourceController(IRegionManager rm, ICsvValidationService csvValidationService,
            ISupervisedDataSetService dataSetService, AppState appState, IFileDialogService fileDialogService)
        {
            _rm = rm;
            _csvValidationService = csvValidationService;
            _dataSetService = dataSetService;
            _appState = appState;
            _fileDialogService = fileDialogService;
        }


        public void Initialize(MultiFileService service)
        {
            _multiFileService = service;


            _multiFileService.ValidateTrainingFile = new DelegateCommand<string>(ValidateTrainingFile);
            _multiFileService.ValidateValidationFile = new DelegateCommand<string>(ValidateValidationFile);
            _multiFileService.ValidateTestFile = new DelegateCommand<string>(ValidateTestFile);

            _multiFileService.SelectTrainingFileCommand = new DelegateCommand(() => SelectMultiSetFile(0));
            _multiFileService.SelectValidationFileCommand = new DelegateCommand(() => SelectMultiSetFile(1));
            _multiFileService.SelectTestFileCommand = new DelegateCommand(() => SelectMultiSetFile(2));

            _multiFileService.LoadFiles =
                new DelegateCommand<(string trainingFile, string validationFile, string testFile)?>(LoadFiles,
                    _ => _loadCanExec);
            _multiFileService.ReturnCommand = new DelegateCommand(() =>
            {
                _trainingData = null;
                _rm.NavigateContentRegion(nameof(SelectDataSourceViewModel), "");
            });
            _multiFileService.ContinueCommand = new DelegateCommand(Continue, () => _continueCanExec);
        }


        private void Continue()
        {
            var vm = MultiFileSourceViewModel.Instance;
            var session = _appState.SessionManager.Create();
            session.TrainingData = _trainingData;
            session.TrainingDataFile = vm.TrainingSetFilePath;
            session.TestDataFile = vm.TestSetFilePath;
            session.ValidationDataFile = vm.ValidationSetFilePath;

            _trainingData = null;
        }


        private void SelectMultiSetFile(int file)
        {
            var dialog = _fileDialogService.OpenCsv();
            if (dialog.result == true)
            {
                if (file == 0)
                {
                    MultiFileSourceViewModel.Instance.TrainingSetFilePath = dialog.filePath;
                }
                else if (file == 1)
                {
                    MultiFileSourceViewModel.Instance.ValidationSetFilePath = dialog.filePath;
                }
                else if (file == 2)
                {
                    MultiFileSourceViewModel.Instance.TestSetFilePath = dialog.filePath;
                }
            }
        }


        private void LoadFiles((string trainingFile, string validationFile, string testFile)? arg)
        {
            _multiFileService.SetIsLoading();

            _trainingData = _dataSetService.LoadDefaultSetsFromFiles(arg.Value.trainingFile, arg.Value.validationFile,
                arg.Value.testFile);
            _continueCanExec = true;
            _multiFileService.ContinueCommand.RaiseCanExecuteChanged();

            _multiFileService.SetLoaded(_trainingData);
        }

        private void ValidateFile(int set, string path)
        {
            var (valid, error, r, c) = _csvValidationService.Validate(path);

            if (!valid)
            {
                if (set == 0) _multiFileService.SetTrainingValidationResult(false, error, true);
                if (set == 1) _multiFileService.SetValidationValidationResult(false, error, true);
                if (set == 2) _multiFileService.SetTestValidationResult(false, error, true);
                _loadCanExec = false;
                _multiFileService.LoadFiles.RaiseCanExecuteChanged();
                return;
            }

            string[] headers = set switch
            {
                0 => _trainingHeaders,
                1 => _validationHeaders,
                2 => _testHeaders,
            };
            string[] headers1 = set switch
            {
                0 => _validationHeaders,
                1 => _trainingHeaders,
                2 => _trainingHeaders,
            };
            string[] headers2 = set switch
            {
                0 => _testHeaders,
                1 => _testHeaders,
                2 => _validationHeaders,
            };

            if (headers1 != null)
            {
                var validCount = headers1.Length == headers.Length;
                if (!validCount)
                {
                    if (set == 0)
                        _multiFileService.SetTrainingValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 1)
                        _multiFileService.SetValidationValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 2)
                        _multiFileService.SetTestValidationResult(false, "Files differ in variables count", r: r, c: c);
                    _loadCanExec = false;
                    _multiFileService.LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                var validContent = !headers.Except(headers1).Any();
                if (!validContent)
                {
                    if (set == 0)
                        _multiFileService.SetTrainingValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    if (set == 1)
                        _multiFileService.SetValidationValidationResult(false, "Files differ in names of variables",
                            r: r, c: c);
                    if (set == 2)
                        _multiFileService.SetTestValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    _loadCanExec = false;
                    _multiFileService.LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                if (set == 0 && !_multiFileService.ValidationValidationResult.HasContentError)
                    _multiFileService.SetValidationValidationResult(true,
                        r: _multiFileService.ValidationValidationResult.Rows,
                        c: _multiFileService.ValidationValidationResult.Cols);

                if ((set == 1 || set == 2) && !_multiFileService.TrainingValidationResult.HasContentError)
                    _multiFileService.SetTrainingValidationResult(true,
                        r: _multiFileService.TrainingValidationResult.Rows,
                        c: _multiFileService.TrainingValidationResult.Cols);
            }

            if (headers2 != null)
            {
                var validCount = headers2.Length == headers.Length;
                if (!validCount)
                {
                    if (set == 0)
                        _multiFileService.SetTrainingValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 1)
                        _multiFileService.SetValidationValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 2)
                        _multiFileService.SetTestValidationResult(false, "Files differ in variables count", r: r, c: c);
                    _loadCanExec = false;
                    _multiFileService.LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                var validContent = !headers.Except(headers2).Any();
                if (!validContent)
                {
                    if (set == 0)
                        _multiFileService.SetTrainingValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    if (set == 1)
                        _multiFileService.SetValidationValidationResult(false, "Files differ in names of variables",
                            r: r, c: c);
                    if (set == 2)
                        _multiFileService.SetTestValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    _loadCanExec = false;
                    _multiFileService.LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                if ((set == 0 || set == 1) && !_multiFileService.TestValidationResult.HasContentError)
                    _multiFileService.SetTestValidationResult(true, r: _multiFileService.TestValidationResult.Rows,
                        c: _multiFileService.TestValidationResult.Cols);
                if (set == 2 && !_multiFileService.ValidationValidationResult.HasContentError)
                    _multiFileService.SetValidationValidationResult(true,
                        r: _multiFileService.ValidationValidationResult.Rows,
                        c: _multiFileService.ValidationValidationResult.Cols);
            }


            if (set == 0) _multiFileService.SetTrainingValidationResult(true, r: r, c: c);
            if (set == 1) _multiFileService.SetValidationValidationResult(true, r: r, c: c);
            if (set == 2) _multiFileService.SetTestValidationResult(true, r: r, c: c);

            if (_multiFileService.TrainingValidationResult.IsFileValid.GetValueOrDefault() &&
                _multiFileService.ValidationValidationResult.IsFileValid.GetValueOrDefault() &&
                _multiFileService.TestValidationResult.IsFileValid.GetValueOrDefault())
            {
                _loadCanExec = true;
                _multiFileService.LoadFiles.RaiseCanExecuteChanged();
            }
        }

        private void ValidateTestFile(string path)
        {
            _multiFileService.SetTestValidating(true);

            _testHeaders = _csvValidationService.ReadHeaders(path);
            ValidateFile(2, path);
        }


        private void ValidateValidationFile(string path)
        {
            _multiFileService.SetValidationValidating(true);

            _validationHeaders = _csvValidationService.ReadHeaders(path);
            ValidateFile(1, path);
        }

        private void ValidateTrainingFile(string path)
        {
            _multiFileService.SetTrainingValidating(true);

            _trainingHeaders = _csvValidationService.ReadHeaders(path);
            ValidateFile(0, path);
        }
    }
}