using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Common.Framework;
using NNLib;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Training.Application.Controllers;
using Training.Application.ViewModels;
using Training.Application.ViewModels.PanelLayout;
using Training.Domain;

namespace Training.Application.Services
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

    public interface ITrainingService : ITransientController
    {
        DelegateCommand StartTrainingSessionCommand { get; }
        DelegateCommand StopTrainingSessionCommand { get; }
        DelegateCommand PauseTrainingSessionCommand { get; }
        DelegateCommand OpenReportsCommand { get;  }
        DelegateCommand OpenParametersCommand { get; }

        DelegateCommand SelectPanelsClickCommand { get; }

        DelegateCommand ResetParametersCommand { get; }

        ModuleStateSessionOptionsDecorator SessionOptionsDecorator { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<ITrainingService, TrainingController>();
        }
    }
}
