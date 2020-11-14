using Common.Domain;
using Common.Framework;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Application.Interfaces;
using Shell.Interface;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Prism.Services.Dialogs;
using Training.Interface;
using Unity;

namespace Shell.Application.ViewModels
{
    public class AboutDialogViewModel : IDialogAware
    {

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

        public string Title { get; } = "About";
        public event Action<IDialogResult> RequestClose;
    }


    public class NavItemIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)value == (int)parameter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool) value ? parameter : -1;
        }
    }

    public class MainWindowViewModel : ViewModelBase<MainWindowViewModel>
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private bool _isFlyoutOpen;
        private string? _flyoutTitle;
        private string _title = "NNLab";
        private bool _isDataItemEnabled;
        private bool _isNetworkItemEnabled;
        private bool _isTrainingItemEnabled;
        private bool _isPredictionItemEnabled;
        private readonly IContentRegionHistoryService _contentRegionHistory;
        private int _checkedNavItemId = -1;

        private AppState _appState;
        private Visibility _modalNavigationVisibility = Visibility.Collapsed;
        private DelegateCommand? _modalNavigationCommand;
        private Visibility _hamburgerMenuVisibility = Visibility.Collapsed;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public MainWindowViewModel()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
        }

        [InjectionConstructor]
        public MainWindowViewModel(IEventAggregator ea, IRegionManager rm, NavigationBreadcrumbsViewModel navigationBreadcrumbsVm, IContentRegionHistoryService contentRegionHistory, AppState appState, StatusBarViewModel statusBarViewModel, IDialogService dialogService)
        {
            _ea = ea;
            _rm = rm;
            NavigationBreadcrumbsVm = navigationBreadcrumbsVm;
            _contentRegionHistory = contentRegionHistory;
            _appState = appState;
            StatusBarViewModel = statusBarViewModel;
            ModalNavigationCommand = new DelegateCommand(() =>
            {
                _modalNavigationCommand?.Execute();
                ModalNavigationVisibility = Visibility.Collapsed;
            });

            ToggleHamburgerMenuCommand = new DelegateCommand(() => HamburgerMenuVisibility = HamburgerMenuVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed);

            ExitCommand = new DelegateCommand(() => System.Windows.Application.Current.MainWindow!.Close());

            AboutCommand = new DelegateCommand(() =>
            {
                dialogService.ShowDialog("AboutDialogView", null, null);
            });

            _ea.GetEvent<ShowFlyout>().Subscribe(args =>
            {
                FlyoutTitle = args.Title;
                IsFlyoutOpen = true;
            }, true);

            _ea.GetEvent<HideFlyout>()
                .Subscribe(() =>
                {
                    CloseFlyoutCommand.Execute(null);
                }, true);


            _ea.GetEvent<EnableNavMenuItem>().Subscribe((identifier) =>
            {
                switch (identifier)
                {
                    case ModuleIds.Data:
                        IsDataItemEnabled = true;
                        break;
                    case ModuleIds.NeuralNetwork:
                        IsNetworkItemEnabled = true;
                        break;
                    case ModuleIds.Training:
                        IsTrainingItemEnabled = true;
                        break;
                    case ModuleIds.Approximation:
                        IsPredictionItemEnabled = true;
                        break;
                }

            });

            _ea.GetEvent<DisableNavMenuItem>().Subscribe((identifier) =>
            {
                switch (identifier)
                {
                    case ModuleIds.Data:
                        IsDataItemEnabled = false;
                        break;
                    case ModuleIds.NeuralNetwork:
                        IsNetworkItemEnabled = false;
                        break;
                    case ModuleIds.Training:
                        IsTrainingItemEnabled = false;
                        break;
                    case ModuleIds.Approximation:
                        IsPredictionItemEnabled = false;
                        break;
                }

            });

            _ea.GetEvent<CheckNavMenuItem>().Subscribe((identifier) =>
            {
                CheckedNavItemId = identifier;
            });

            _ea.GetEvent<TrainingSessionStarted>().Subscribe(() =>
                {
                    IsDataItemEnabled = IsNetworkItemEnabled = IsPredictionItemEnabled = false;
                });
            _ea.GetEvent<TrainingSessionPaused>().Subscribe(() =>
            {
                IsDataItemEnabled = IsNetworkItemEnabled = IsPredictionItemEnabled = true;
            });
            _ea.GetEvent<TrainingSessionStopped>().Subscribe(() =>
            {
                IsDataItemEnabled = IsNetworkItemEnabled = IsPredictionItemEnabled = true;
            });

            _ea.GetEvent<EnableModalNavigation>().Subscribe(command =>
                {
                    ModalNavigationVisibility = Visibility.Visible;
                    _modalNavigationCommand = command;
                });

            _ea.GetEvent<DisableModalNavigation>().Subscribe(() =>
            {
                _modalNavigationCommand = null;
                ModalNavigationVisibility = Visibility.Collapsed;
            });

            CloseFlyoutCommand = new DelegateCommand(() =>
            {
                IsFlyoutOpen = false;
                _rm.Regions[AppRegions.FlyoutRegion].RemoveAll();
            });


            _appState.ActiveSessionChanged += AppStateOnActiveSessionChanged;
        }

        private void AppStateOnActiveSessionChanged(object? sender, (Session? prev, Session next) e)
        {
            //if not changed to first created session
            if (e.prev != null)
            {
                _contentRegionHistory.ClearHistoryForModulesExcept(CheckedNavItemId);
                NavigationBreadcrumbsVm.ClearPreviousBreadcrumbsExcept(CheckedNavItemId);

                //for data, network and training module ids
                for (int i = 1; i <= 4; i++)
                {
                    if(i != CheckedNavItemId) _ea.GetEvent<SetupNewNavigationForSession>().Publish((i, e.prev,e.next));
                }

                _ea.GetEvent<ReloadContentForSession>().Publish((CheckedNavItemId, e.prev, e.next));
            }
        }

        public NavigationBreadcrumbsViewModel NavigationBreadcrumbsVm { get; }
        public StatusBarViewModel StatusBarViewModel { get; }
        public DelegateCommand ToggleHamburgerMenuCommand { get; }
        public DelegateCommand ExitCommand { get; }
        public DelegateCommand AboutCommand { get; }

        private void Navigate(int identifier)
        {
            if (CheckedNavItemId != -1)
            {
                NavigationBreadcrumbsVm.SaveBreadcrumbsForModule(CheckedNavItemId);
                _contentRegionHistory.SaveContentForModule(CheckedNavItemId);
            }
            _ea.GetEvent<PreviewCheckNavMenuItem>().Publish(new PreviewCheckNavMenuItemArgs(CheckedNavItemId, identifier));

            _contentRegionHistory.TryRestoreContentForModule(identifier);
            NavigationBreadcrumbsVm.TryRestoreBreadrumbsForModule(identifier);
        }


        public ICommand CloseFlyoutCommand { get; set; }

        public bool IsFlyoutOpen
        {
            get => _isFlyoutOpen;
            set => SetProperty(ref _isFlyoutOpen, value);
        }

        public string? FlyoutTitle
        {
            get => _flyoutTitle;
            set => SetProperty(ref _flyoutTitle, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }


        public int CheckedNavItemId
        {
            get => _checkedNavItemId;
            set
            {
                if(value != -1)
                {
                    Navigate(value);
                    SetProperty(ref _checkedNavItemId, value);
                }
            }
        }

        public bool IsDataItemEnabled
        {
            get => _isDataItemEnabled;
            set => SetProperty(ref _isDataItemEnabled, value);
        }

        public bool IsNetworkItemEnabled
        {
            get => _isNetworkItemEnabled;
            set => SetProperty(ref _isNetworkItemEnabled, value);
        }

        public bool IsTrainingItemEnabled
        {
            get => _isTrainingItemEnabled;
            set => SetProperty(ref _isTrainingItemEnabled, value);
        }

        public bool IsPredictionItemEnabled
        {
            get => _isPredictionItemEnabled;
            set => SetProperty(ref _isPredictionItemEnabled, value);
        }

        public Visibility ModalNavigationVisibility
        {
            get => _modalNavigationVisibility;
            set
            {
                SetProperty(ref _modalNavigationVisibility, value);
                if (value == Visibility.Visible)
                {
                    StatusBarViewModel.CanModifyActiveSession = false;
                }
                else
                {
                    StatusBarViewModel.CanModifyActiveSession = true;
                }
            }
        }

        public DelegateCommand ModalNavigationCommand { get; }

        public Visibility HamburgerMenuVisibility
        {
            get => _hamburgerMenuVisibility;
            set => SetProperty(ref _hamburgerMenuVisibility, value);
        }
    }
}