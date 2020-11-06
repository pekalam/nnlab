using Common.Domain;
using Common.Framework;
using Prism.Events;
using Shell.Application.Services;
using Shell.Interface;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Shell.Application.ViewModels
{
    public class StatusBarViewModel : ViewModelBase<StatusBarViewModel>
    {
        private Visibility _errorNotificationVisibility = Visibility.Collapsed;
        private string? _errorMessage = "Invalid error";
        private string? _networkInfo;
        private readonly StringBuilder _bldr = new StringBuilder();
        private Visibility _progressVisibility = Visibility.Hidden;
        private string? _progressTooltip;
        private string? _progressMessage;
        private bool _canModifyActiveSession = true;
        private readonly AppState _appState;

#pragma warning disable 8618
        public StatusBarViewModel()
#pragma warning restore 8618
        {
        }

        public StatusBarViewModel(AppState appState, IStatusBarService serivce, IEventAggregator ea)
        {
            AppState = appState;
            _appState = appState;
            Serivce = serivce;

            ea.GetEvent<ShowProgressArea>().Subscribe(args =>
            {
                ProgressMessage = args.Message;
                ProgressTooltip = args.Tooltip;
                ProgressVisibility = Visibility.Visible;
            }, ThreadOption.UIThread);

            ea.GetEvent<HideProgressArea>().Subscribe(args =>
            {
                ProgressVisibility = Visibility.Hidden;
            }, ThreadOption.UIThread);

            appState.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AppState.ActiveSession))
                {
                    if(_appState.ActiveSession!.Network != null) UpdateNetworkInfo();
                    _appState.ActiveSession.NetworkStructureChanged += _ => UpdateNetworkInfo();
                    _appState.ActiveSession.PropertyChanged += ActiveSessionOnPropertyChanged;
                }
            };

            serivce.Initialize(this);
        }

        private void UpdateNetworkInfo()
        {
            var network = AppState.ActiveSession!.Network;
            _bldr.Clear();

            if (network == null) return;

            for (int i = 0; i < network.Layers.Count; i++)
            {
                var layer = network.Layers[i];
                if (i == 0)
                {
                    _bldr.Append(layer.InputsCount);
                    _bldr.Append(" x ");
                }

                _bldr.Append(layer.NeuronsCount);
                if (i != network.Layers.Count - 1)
                {
                    _bldr.Append(" x ");
                }
            }

            NetworkInfo = _bldr.ToString();
        }

        private void ActiveSessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var session = (sender as Session)!;
            if (e.PropertyName == nameof(Session.Network) && session.Network != null)
            {
                //todo remove handler
                UpdateNetworkInfo();
            }
        }

        public IStatusBarService Serivce { get; }

        public AppState AppState { get; }

        public Visibility ErrorNotificationVisibility
        {
            get => _errorNotificationVisibility;
            set => SetProperty(ref _errorNotificationVisibility, value);
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string? NetworkInfo
        {
            get => _networkInfo;
            set => SetProperty(ref _networkInfo, value);
        }

        public bool CanModifyActiveSession
        {
            get => _canModifyActiveSession;
            set => SetProperty(ref _canModifyActiveSession, value);
        }


        public Visibility ProgressVisibility
        {
            get => _progressVisibility;
            set => SetProperty(ref _progressVisibility, value);
        }

        public string? ProgressTooltip
        {
            get => _progressTooltip;
            set => SetProperty(ref _progressTooltip, value);
        }

        public string? ProgressMessage
        {
            get => _progressMessage;
            set => SetProperty(ref _progressMessage, value);
        }
    }

    public class FileNameConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;

            return str?.Split('\\')[^1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
