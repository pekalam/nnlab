using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Accessibility;
using Common.Framework;
using NNLib;
using NNLib.Exceptions;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Shell.Interface;
using Training.Application.ViewModels;
using Training.Application.ViewModels.PanelLayout;
using Training.Domain;

namespace Training.Application.Controllers
{
    public class ModuleStateSessionOptionsDecorator : BindableBase
    {
        private TrainingSession? _session;
        private readonly IMetroDialogService _dialogService;

        public ModuleStateSessionOptionsDecorator(ModuleStateHelper moduleStateHelper, IMetroDialogService dialogService)
        {
            _dialogService = dialogService;
            moduleStateHelper.OnActiveSessionChanged(session => Session = session);
            ResetSessionCommand = new DelegateCommand(ResetSession);
        }

        public ICommand ResetSessionCommand { get; }

        public TrainingSession? Session
        {
            get => _session;
            set => SetProperty(ref _session, value);
        }

        private async void ResetSession()
        {
            Debug.Assert(_session != null);
            if (_dialogService.ShowModalConfirmationDialog("Confirm session reset", "This action will remove all reports and set all network parameters to state before training was started."))
            {
                await _session.ResetSession();
            }
        }
    }

    public interface ITrainingController : ITransientController
    {
        DelegateCommand StartTrainingSessionCommand { get; }
        DelegateCommand StopTrainingSessionCommand { get; }
        DelegateCommand PauseTrainingSessionCommand { get; }
        DelegateCommand OpenReportsCommand { get; }
        DelegateCommand OpenParametersCommand { get; }

        DelegateCommand SelectPanelsClickCommand { get; }

        DelegateCommand ResetParametersCommand { get; }

        ModuleStateSessionOptionsDecorator SessionOptionsDecorator { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ITrainingController, TrainingController>();
        }
    }


    class TrainingController : ControllerBase<TrainingViewModel>,ITrainingController
    {
        private readonly IRegionManager _rm;
        private readonly IEventAggregator _ea;
        private readonly IDialogService _dialogService;
        private List<PanelSelectModel> _lastSelectedPanels = new List<PanelSelectModel>();

        private readonly ModuleState _moduleState;
        private readonly ModuleStateHelper _helper;

        public TrainingController(IRegionManager rm, IEventAggregator ea, IDialogService dialogService, ModuleState moduleState, ModuleStateSessionOptionsDecorator sessionOptionsDecorator)
        {
            _rm = rm;
            _ea = ea;
            _dialogService = dialogService;
            _moduleState = moduleState;

            SessionOptionsDecorator = sessionOptionsDecorator;
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

            void CheckCommandsCanExec()
            {
                StartTrainingSessionCommand.RaiseCanExecuteChanged();
                StopTrainingSessionCommand.RaiseCanExecuteChanged();
                PauseTrainingSessionCommand.RaiseCanExecuteChanged();
                OpenReportsCommand.RaiseCanExecuteChanged();
                SelectPanelsClickCommand.RaiseCanExecuteChanged();
                OpenParametersCommand.RaiseCanExecuteChanged();
            }

            // _moduleState.ActiveSessionChanged += (_, args) =>
            // {
            //     CheckCommandsCanExec();
            //
            //     if (args.prev != null)
            //     {
            //         //TODO epoch end consumer
            //         if (_rm.Regions.ContainsRegionWithName(TrainingViewRegions.PanelLayoutRegion))
            //         {
            //             //_rm.Regions[TrainingViewRegions.PanelLayoutRegion].RemoveAll();
            //             // _service.HidePanels();
            //         }
            //     }
            //
            //
            // };

            _helper = new ModuleStateHelper(moduleState);

            _helper.OnActiveSessionChanged(session =>
            {
                CheckCommandsCanExec();

                Session.PropertyChanged += (a, b) => CheckCommandsCanExec();
                Session.SessionReset += () =>
                {
                    Vm!.HidePanels();
                    CheckCommandsCanExec();
                };
            });

        }

        private void ResetParameters()
        {
            Debug.Assert(_moduleState.ActiveSession?.Network != null);
            _moduleState.ActiveSession!.Network.ResetParameters();
        }

        private async void PauseTrainingSession()
        {
            await Session.Pause();
        }

        private async void StopTrainingSession()
        {
            await Session.Stop();
        }

        private TrainingSession Session => _moduleState.ActiveSession!;

        private async void StartTrainingSession()
        {
            _ea.GetEvent<ShowProgressArea>().Publish(new ProgressAreaArgs
                {
                    Tooltip = "Training in progress",
                    Message = "Training"
                });
            await Session.Start();

            _ea.GetEvent<HideProgressArea>().Publish(null);
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
        public DelegateCommand OpenReportsCommand { get; }
        public DelegateCommand OpenParametersCommand { get; }
        public DelegateCommand SelectPanelsClickCommand { get; }
        public DelegateCommand ResetParametersCommand { get; }
        public ModuleStateSessionOptionsDecorator SessionOptionsDecorator { get; }
    }
}