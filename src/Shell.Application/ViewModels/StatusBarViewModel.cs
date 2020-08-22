using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using Common.Domain;
using Common.Framework;
using Shell.Application.Services;

namespace Shell.Application.ViewModels
{
    public class StatusBarViewModel : ViewModelBase<StatusBarViewModel>
    {
        private Visibility _errorNotificationVisibility = Visibility.Collapsed;
        private string _errorMessage = "Invalid error";
        private string _networkInfo;
        private StringBuilder _bldr = new StringBuilder();

        public StatusBarViewModel()
        {
        }

        public StatusBarViewModel(AppState appState, IStatusBarService serivce)
        {
            AppState = appState;
            Serivce = serivce;

            appState.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AppState.ActiveSession))
                {
                    appState.ActiveSession.PropertyChanged += ActiveSessionOnPropertyChanged;
                }
            };
        }

        private void UpdateNetworkInfo()
        {
            var network = AppState.ActiveSession.Network;
            _bldr.Clear();

            if (network == null) return;

            for (int i = 0; i < network.Layers.Count; i++)
            {
                var layer = network.Layers[i];
                if (layer.IsInputLayer)
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
            var session = sender as Session;
            if (e.PropertyName == nameof(Session.Network) && session.Network != null)
            {
                //todo remove handler
                UpdateNetworkInfo();
                session.NetworkStructureChanged += _ => UpdateNetworkInfo();
            }
        }

        public IStatusBarService Serivce { get; }

        public AppState AppState { get; }

        public Visibility ErrorNotificationVisibility
        {
            get => _errorNotificationVisibility;
            set => SetProperty(ref _errorNotificationVisibility, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string NetworkInfo
        {
            get => _networkInfo;
            set => SetProperty(ref _networkInfo, value);
        }
    }

    public class FileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
