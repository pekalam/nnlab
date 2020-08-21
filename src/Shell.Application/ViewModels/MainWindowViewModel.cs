using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;
using Common.Domain;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Shell.Application.Interfaces;
using Shell.Interface;
using Unity;

namespace Shell.Application.ViewModels
{
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

    public class MainWindowViewModel : BindableBase
    {
        private readonly IEventAggregator _ea;
        private readonly IRegionManager _rm;
        private bool _isFlyoutOpen;
        private string _flyoutTitle;
        private string _title = "NNLab";
        private bool _isDataItemEnabled;
        private bool _isNetworkItemEnabled;
        private bool _isTrainingItemEnabled;
        private bool _isPredictionItemEnabled;
        private readonly IContentRegionHistoryService _contentRegionHistory;
        private int _checkedNavItemId = -1;

        public MainWindowViewModel()
        {
        }

        [InjectionConstructor]
        public MainWindowViewModel(IEventAggregator ea, IRegionManager rm, NavigationBreadcrumbsViewModel navigationBreadcrumbsVm, IContentRegionHistoryService contentRegionHistory)
        {
            _ea = ea;
            _rm = rm;
            NavigationBreadcrumbsVm = navigationBreadcrumbsVm;
            _contentRegionHistory = contentRegionHistory;

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
                }

            });

            _ea.GetEvent<CheckNavMenuItem>().Subscribe((identifier) =>
            {
                CheckedNavItemId = identifier;
            });


            CloseFlyoutCommand = new DelegateCommand(() =>
            {
                IsFlyoutOpen = false;
                _rm.Regions[AppRegions.FlyoutRegion].RemoveAll();
            });
        }

        public NavigationBreadcrumbsViewModel NavigationBreadcrumbsVm { get; }

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

        public string FlyoutTitle
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
    }
}