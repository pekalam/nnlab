using Common.Domain;
using Common.Framework;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;
using Unity;

namespace Shell.Application.ViewModels
{
    public class DuplicateSessionDialogViewModel : ViewModelBase<DuplicateSessionDialogViewModel>, IDialogAware
    {
        private bool _includeData = true;
        private bool _includeNetwork = true;
        private bool _includeTrainingParameters = true;

        private DuplicateOptions Options { get; set; } = DuplicateOptions.All;


#pragma warning disable 8618
        public DuplicateSessionDialogViewModel()
#pragma warning restore 8618
        {
            
        }

        [InjectionConstructor]
        public DuplicateSessionDialogViewModel(AppState appState)
        {
            AppState = appState;
            OkCommand = new DelegateCommand(() =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, new DialogParameters()
                {
                    {"options", Options}
                }));
            });

            CancelCommand = new DelegateCommand(() =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            });
        }


        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public AppState AppState { get; }

        private void TryResetToDefault()
        {
            if (IncludeData && IncludeNetwork && IncludeTrainingParameters)
            {
                Options = DuplicateOptions.All;
            }
        }

        public bool IncludeData
        {
            get => _includeData;
            set
            {
                SetProperty(ref _includeData, value);
                if (!value)
                {
                    Options |= DuplicateOptions.NoData;
                }
                else
                {
                    Options &= ~DuplicateOptions.NoData;
                    TryResetToDefault();
                }
            }
        }

        public bool IncludeNetwork
        {
            get => _includeNetwork;
            set
            {
                SetProperty(ref _includeNetwork, value);
                if (!value)
                {
                    Options |= DuplicateOptions.NoNetwork;
                }
                else
                {
                    Options &= ~DuplicateOptions.NoNetwork;
                    TryResetToDefault();
                }
            }
        }

        public bool IncludeTrainingParameters
        {
            get => _includeTrainingParameters;
            set
            {
                SetProperty(ref _includeTrainingParameters, value);
                if (!value)
                {
                    Options |= DuplicateOptions.NoTrainingParams;
                }
                else
                {
                    Options &= ~DuplicateOptions.NoTrainingParams;
                    TryResetToDefault();
                }
            }
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public string Title { get; } = "Duplicate session";
        public event Action<IDialogResult>? RequestClose;
    }
}
