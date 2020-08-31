using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
using Common.Framework;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Shell.Application.ViewModels
{
    public class CreateSessionDialogViewModel : ViewModelBase<CreateSessionDialogViewModel>, IDialogAware
    {
        private string _name;
        private bool _isSwitchToSessionChecked = true;

        public CreateSessionDialogViewModel()
        {
            OkCommand = new DelegateCommand(() => RequestClose(new DialogResult(ButtonResult.OK, new DialogParameters()
            {
                {nameof(Name), Name ?? ""},
                {"Switch", IsSwitchToSessionChecked}
            })));
            CancelCommand = new DelegateCommand(() => RequestClose(new DialogResult(ButtonResult.Cancel)));
        }


        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public bool IsSwitchToSessionChecked
        {
            get => _isSwitchToSessionChecked;
            set => SetProperty(ref _isSwitchToSessionChecked, value);
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

        public string Title { get; } = "Create session";
        public event Action<IDialogResult> RequestClose;
    }
}
