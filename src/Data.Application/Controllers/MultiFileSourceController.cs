using Common.Domain;
using Common.Framework;
using Data.Application.Interfaces;
using Data.Application.ViewModels;
using Data.Domain.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using System;
using System.Linq;

namespace Data.Application.Controllers
{
    public interface IMultiFileSourceController : IController
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

    internal class MultiFileSourceController : ControllerBase<MultiFileSourceViewModel>, IMultiFileSourceController
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private readonly ICsvValidationService _csvValidationService;
        private readonly ITrainingDataService _dataService;
        private readonly IFileDialogService _fileDialogService;
        private readonly AppState _appState;

        private TrainingData? _trainingData;

        private string[]? _trainingHeaders;
        private string[]? _validationHeaders;
        private string[]? _testHeaders;

        private bool _loadCanExec;
        private bool _continueCanExec;


        public MultiFileSourceController(IRegionManager rm, ICsvValidationService csvValidationService,
            ITrainingDataService dataService, AppState appState, IFileDialogService fileDialogService, IEventAggregator ea)
        {
            _rm = rm;
            _csvValidationService = csvValidationService;
            _dataService = dataService;
            _appState = appState;
            _fileDialogService = fileDialogService;
            _ea = ea;

            ValidateTrainingFile = new DelegateCommand<string>(ValidateTrainingFileExecute);
            ValidateValidationFile = new DelegateCommand<string>(ValidateValidationFileExecute);
            ValidateTestFile = new DelegateCommand<string>(ValidateTestFileExecute);

            SelectTrainingFileCommand = new DelegateCommand(() => SelectMultiSetFile(0));
            SelectValidationFileCommand = new DelegateCommand(() => SelectMultiSetFile(1));
            SelectTestFileCommand = new DelegateCommand(() => SelectMultiSetFile(2));

            LoadFiles =
                new DelegateCommand<(string trainingFile, string validationFile, string testFile)?>(LoadFilesExecute,
                    _ => _loadCanExec);
            ReturnCommand = new DelegateCommand(() =>
            {
                _trainingData = null;
                _rm.NavigateContentRegion("SelectDataSourceView");
            });
            ContinueCommand = new DelegateCommand(Continue, () => _continueCanExec);

            _ea.GetEvent<EnableModalNavigation>().Publish(ReturnCommand);
        }


        private void Continue()
        {
            if (_appState.Sessions.Count == 0)
            {
                _appState.CreateSession();
            }
            _appState.ActiveSession!.TrainingData = _trainingData;
            _appState.ActiveSession.TrainingDataFile = Vm!.TrainingSetFilePath;
            _appState.ActiveSession.TestDataFile = Vm!.TestSetFilePath;
            _appState.ActiveSession.ValidationDataFile = Vm!.ValidationSetFilePath;

            _trainingData = null;

            _ea.GetEvent<DisableModalNavigation>().Publish();
            _rm.NavigateContentRegion("FileDataSourceView", new NavigationParameters("Files")
            {
                {"Multi", true}
            });
        }


        private void SelectMultiSetFile(int file)
        {
            var dialog = _fileDialogService.OpenCsv();
            if (dialog.result == true)
            {
                if (file == 0)
                {
                    Vm!.TrainingSetFilePath = dialog.filePath!;
                }
                else if (file == 1)
                {
                    Vm!.ValidationSetFilePath = dialog.filePath!;
                }
                else if (file == 2)
                {
                    Vm!.TestSetFilePath = dialog.filePath!;
                }
            }
        }


        private void LoadFilesExecute((string trainingFile, string validationFile, string testFile)? arg)
        {
            Vm!.SetIsLoading();

            _trainingData = _dataService.LoadDefaultTrainingDataFromFiles(arg!.Value.trainingFile, arg.Value.validationFile,
                arg.Value.testFile);
            _continueCanExec = true;
            ContinueCommand.RaiseCanExecuteChanged();

            Vm!.SetLoaded(_trainingData);
        }

        private void ValidateFile(int set, string path)
        {
            var (valid, error, r, c) = _csvValidationService.Validate(path);

            if (!valid)
            {
                if (set == 0) Vm!.SetTrainingValidationResult(false, error, true);
                if (set == 1) Vm!.SetValidationValidationResult(false, error, true);
                if (set == 2) Vm!.SetTestValidationResult(false, error!, true);
                _loadCanExec = false;
                LoadFiles.RaiseCanExecuteChanged();
                return;
            }

            string[] headers = set switch
            {
                0 => _trainingHeaders!,
                1 => _validationHeaders!,
                2 => _testHeaders!,
                _ => throw new NotImplementedException()
            };
            string[]? headers1 = set switch
            {
                0 => _validationHeaders,
                1 => _trainingHeaders,
                2 => _trainingHeaders,
                _ => throw new NotImplementedException()
            };
            string[]? headers2 = set switch
            {
                0 => _testHeaders,
                1 => _testHeaders,
                2 => _validationHeaders,
                _ => throw new NotImplementedException()
            };

            if (headers1 != null)
            {
                var validCount = headers1.Length == headers.Length;
                if (!validCount)
                {
                    if (set == 0)
                        Vm!.SetTrainingValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 1)
                        Vm!.SetValidationValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 2)
                        Vm!.SetTestValidationResult(false, "Files differ in variables count", r: r, c: c);
                    _loadCanExec = false;
                    LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                var validContent = !headers.Except(headers1).Any();
                if (!validContent)
                {
                    if (set == 0)
                        Vm!.SetTrainingValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    if (set == 1)
                        Vm!.SetValidationValidationResult(false, "Files differ in names of variables",
                            r: r, c: c);
                    if (set == 2)
                        Vm!.SetTestValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    _loadCanExec = false;
                    LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                if (set == 0 && !Vm!.ValidationValidationResult.HasContentError)
                    Vm!.SetValidationValidationResult(true,
                        r: Vm!.ValidationValidationResult.Rows,
                        c: Vm!.ValidationValidationResult.Cols);

                if ((set == 1 || set == 2) && !Vm!.TrainingValidationResult.HasContentError)
                    Vm!.SetTrainingValidationResult(true,
                        r: Vm!.TrainingValidationResult.Rows,
                        c: Vm!.TrainingValidationResult.Cols);
            }

            if (headers2 != null)
            {
                var validCount = headers2.Length == headers.Length;
                if (!validCount)
                {
                    if (set == 0)
                        Vm!.SetTrainingValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 1)
                        Vm!.SetValidationValidationResult(false, "Files differ in variables count", r: r,
                            c: c);
                    if (set == 2)
                        Vm!.SetTestValidationResult(false, "Files differ in variables count", r: r, c: c);
                    _loadCanExec = false;
                    LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                var validContent = !headers.Except(headers2).Any();
                if (!validContent)
                {
                    if (set == 0)
                        Vm!.SetTrainingValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    if (set == 1)
                        Vm!.SetValidationValidationResult(false, "Files differ in names of variables",
                            r: r, c: c);
                    if (set == 2)
                        Vm!.SetTestValidationResult(false, "Files differ in names of variables", r: r,
                            c: c);
                    _loadCanExec = false;
                    LoadFiles.RaiseCanExecuteChanged();
                    return;
                }

                if ((set == 0 || set == 1) && !Vm!.TestValidationResult.HasContentError)
                    Vm!.SetTestValidationResult(true, r: Vm!.TestValidationResult.Rows,
                        c: Vm!.TestValidationResult.Cols);
                if (set == 2 && !Vm!.ValidationValidationResult.HasContentError)
                    Vm!.SetValidationValidationResult(true,
                        r: Vm!.ValidationValidationResult.Rows,
                        c: Vm!.ValidationValidationResult.Cols);
            }


            if (set == 0) Vm!.SetTrainingValidationResult(true, r: r, c: c);
            if (set == 1) Vm!.SetValidationValidationResult(true, r: r, c: c);
            if (set == 2) Vm!.SetTestValidationResult(true, r: r, c: c);

            if (Vm!.TrainingValidationResult.IsFileValid.GetValueOrDefault() &&
                Vm!.ValidationValidationResult.IsFileValid.GetValueOrDefault() &&
                Vm!.TestValidationResult.IsFileValid.GetValueOrDefault())
            {
                _loadCanExec = true;
                LoadFiles.RaiseCanExecuteChanged();
            }
        }

        private void ValidateTestFileExecute(string path)
        {
            Vm!.SetTestValidating(true);

            _testHeaders = _csvValidationService.ReadHeaders(path);
            ValidateFile(2, path);
        }


        private void ValidateValidationFileExecute(string path)
        {
            Vm!.SetValidationValidating(true);

            _validationHeaders = _csvValidationService.ReadHeaders(path);
            ValidateFile(1, path);
        }

        private void ValidateTrainingFileExecute(string path)
        {
            Vm!.SetTrainingValidating(true);

            _trainingHeaders = _csvValidationService.ReadHeaders(path);
            ValidateFile(0, path);
        }

        public DelegateCommand SelectTrainingFileCommand { get; set; }
        public DelegateCommand SelectValidationFileCommand { get; set; }
        public DelegateCommand SelectTestFileCommand { get; set; }
        public DelegateCommand ReturnCommand { get; set; }
        public DelegateCommand ContinueCommand { get; set; }
        public DelegateCommand<string> ValidateTrainingFile { get; set; }
        public DelegateCommand<string> ValidateTestFile { get; set; }
        public DelegateCommand<string> ValidateValidationFile { get; set; }
        public DelegateCommand<(string trainingFile, string validationFile, string testFile)?> LoadFiles { get; set; }
    }
}