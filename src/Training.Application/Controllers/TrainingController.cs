using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Accessibility;
using Common.Framework;
using NNLib;
using NNLib.Exceptions;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Shell.Interface;
using Training.Application.Services;
using Training.Application.ViewModels;
using Training.Application.ViewModels.PanelLayout;
using Training.Domain;

namespace Training.Application.Controllers
{
    class TrainingController : ControllerBase<TrainingViewModel>,ITrainingService
    {
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;
        private readonly IDialogService _dialogService;
        private List<PanelSelectModel> _lastSelectedPanels = new List<PanelSelectModel>();

        private readonly ModuleState _moduleState;

        public TrainingController(IRegionManager rm, IEventAggregator ea, IDialogService dialogService, ModuleState moduleState)
        {
            _rm = rm;
            _ea = ea;
            _dialogService = dialogService;
            _moduleState = moduleState;

            OpenParametersCommand = new DelegateCommand(OpenParameters,
                () => _moduleState.ActiveSession != null && !_moduleState.ActiveSession.Started);
            OpenReportsCommand = new DelegateCommand(OpenReports,
                () => _moduleState.ActiveSession?.CurrentReport != null && !_moduleState.ActiveSession.Started);
            SelectPanelsClickCommand = new DelegateCommand(SelectPanels,
                () => _moduleState.ActiveSession != null && _moduleState.ActiveSession.IsValid);
            StartTrainingSessionCommand = new DelegateCommand(StartTrainingSession, () =>
                _moduleState.ActiveSession != null && _moduleState.ActiveSession.IsValid &&
                !_moduleState.ActiveSession.Stopped && !_moduleState.ActiveSession.Started);
            StopTrainingSessionCommand = new DelegateCommand(StopTrainingSession, () =>
                _moduleState.ActiveSession != null && _moduleState.ActiveSession.IsValid &&
                !_moduleState.ActiveSession.Stopped && _moduleState.ActiveSession.Started);
            PauseTrainingSessionCommand = new DelegateCommand(PauseTrainingSession, () =>
                _moduleState.ActiveSession != null && _moduleState.ActiveSession.IsValid &&
                !_moduleState.ActiveSession.Stopped && _moduleState.ActiveSession.Started);
            ResetParametersCommand = new DelegateCommand(ResetParameters,
                () => _moduleState.ActiveSession != null && _moduleState.ActiveSession.IsValid);
            RunTestCommand = new DelegateCommand(RunTest, () =>
                _moduleState.ActiveSession != null && _moduleState.ActiveSession.IsValid &&
                _moduleState.ActiveSession.TrainingData!.Sets.TestSet != null && !_moduleState.ActiveSession.Started &&
                _moduleState.ActiveSession.CurrentReport != null);
            RunValidationCommand = new DelegateCommand(RunValidation, () =>
                _moduleState.ActiveSession != null && _moduleState.ActiveSession.IsValid &&
                _moduleState.ActiveSession.TrainingData!.Sets.ValidationSet != null &&
                !_moduleState.ActiveSession.Started && _moduleState.ActiveSession.CurrentReport != null);

            void CheckCommandsCanExec()
            {
                StartTrainingSessionCommand.RaiseCanExecuteChanged();
                StopTrainingSessionCommand.RaiseCanExecuteChanged();
                PauseTrainingSessionCommand.RaiseCanExecuteChanged();
                OpenReportsCommand.RaiseCanExecuteChanged();
                SelectPanelsClickCommand.RaiseCanExecuteChanged();
                OpenParametersCommand.RaiseCanExecuteChanged();
                RunTestCommand.RaiseCanExecuteChanged();
                RunValidationCommand.RaiseCanExecuteChanged();
            }

            _moduleState.ActiveSessionChanged += (_, args) =>
            {
                CheckCommandsCanExec();

                if (args.prev != null)
                {
                    //TODO epoch end consumer
                    if (_rm.Regions.ContainsRegionWithName(TrainingViewRegions.PanelLayoutRegion))
                    {
                        //_rm.Regions[TrainingViewRegions.PanelLayoutRegion].RemoveAll();
                        // _service.HidePanels();
                    }
                }

                Session.PropertyChanged += (a, b) => CheckCommandsCanExec();
                Session.SessionReset += () =>
                {
                    Vm!.HidePanels();
                    CheckCommandsCanExec();
                };
            };
        }

        private void RunValidation()
        {
            throw new NotImplementedException();
        }

        private void RunTest()
        {
            throw new NotImplementedException();
        }

        private void ResetParameters()
        {
            Debug.Assert(_moduleState.ActiveSession?.Network != null);
            _moduleState.ActiveSession!.Network.ResetParameters();
        }

        private async void PauseTrainingSession()
        {
            try
            {
                await Session.Pause();
            }
            catch (OperationCanceledException)
            {
            }
            catch (TrainingCanceledException)
            {
            }
        }

        private async void StopTrainingSession()
        {
            try
            {
                await Session.Stop();
            }
            catch (OperationCanceledException)
            {
            }
            catch (TrainingCanceledException)
            {
            }
        }

        private TrainingSession Session => _moduleState.ActiveSession!;

        private async void StartTrainingSession()
        {
            try
            {
                _ea.GetEvent<ShowProgressArea>().Publish(new ProgressAreaArgs()
                {
                    Tooltip = "Training in progress",
                    Message = "Training"
                });
                await Session.Start();
            }
            catch (OperationCanceledException)
            {
            }
            catch (TrainingCanceledException)
            {
            }
            finally
            {
                _ea.GetEvent<HideProgressArea>().Publish(null);
            }
        }

        private void SelectPanels()
        {
            var parameters = new DialogParameters()
            {
                {"single", false},
                {"maxSelected", 4},
                {nameof(PanelSelectionResult), new PanelSelectionResult(OnPanelsSelected)}
            };

            if (_lastSelectedPanels.Count > 0)
            {
                parameters.Add("selected", _lastSelectedPanels.Select(v => v.PanelType).ToArray());
            }

            _dialogService.ShowDialog("PanelSelectView", parameters, null);
        }

        private void OnPanelsSelected(List<PanelSelectModel> list)
        {
            _lastSelectedPanels = list;

            if (list.Count > 0)
            {
                var param = new PanelLayoutNavigationParams(list, new List<(string name, string value)>()
                {
                    ("ParentRegion", TrainingViewRegions.PanelLayoutRegion)
                });
                Vm!.ShowPanels(param);
            }
            else
            {
                Vm!.HidePanels();
            }
        }

        private void OpenReports()
        {
            _rm.NavigateContentRegion("ReportsView");
        }

        private void OpenParameters()
        {
            _rm.NavigateContentRegion("TrainingParametersView");
        }

        public DelegateCommand StartTrainingSessionCommand { get; }
        public DelegateCommand StopTrainingSessionCommand { get; }
        public DelegateCommand PauseTrainingSessionCommand { get; }
        public DelegateCommand RunValidationCommand { get; }
        public DelegateCommand RunTestCommand { get; }
        public DelegateCommand OpenReportsCommand { get; }
        public DelegateCommand OpenParametersCommand { get; }
        public DelegateCommand SelectPanelsClickCommand { get; }
        public DelegateCommand ResetParametersCommand { get; }
        public ModuleStateSessionOptionsDecorator SessionOptionsDecorator { get; }
    }
}