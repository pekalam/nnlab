using NNLib.MLP;
using System;
using System.ComponentModel;

namespace Common.Domain
{
    public class AppStateHelper
    {
        private readonly AppState _appState;

        private EventHandler<(Session? prev, Session next)> _netActiveSessionChanged = null!;
        private PropertyChangedEventHandler _activeSessionNetChanged = null!;

        private EventHandler<(Session? prev, Session next)> _dataActiveSessionInSession = null!;
        private PropertyChangedEventHandler _activeSessionDataInSession = null!;
        private EventHandler<(Session? prev, Session next)> _dataActiveSessionChanged = null!;
        private PropertyChangedEventHandler _activeSessionDataChanged = null!;
        private EventHandler<(Session? prev, Session next)> _dataActiveSessionPropertyChanged = null!;
        private PropertyChangedEventHandler _activeSessionDataPropertyChanged = null!;

        private PropertyChangedEventHandler? _dataPropertyChanged = null!;

        public event EventHandler<(Session? prev, Session next)>? _activeSessionChanged;

        public AppStateHelper(AppState appState)
        {
            _appState = appState;
        }

        public void OnSessionChangedOrSet(Action<Session> sessionChangedOrSet)
        {
            _activeSessionChanged = (_, args) => sessionChangedOrSet(args.next);
            _appState.ActiveSessionChanged -= _activeSessionChanged;
            _appState.ActiveSessionChanged += _activeSessionChanged;

            if (_appState.ActiveSession != null)
            {
                sessionChangedOrSet(_appState.ActiveSession);
            }
        }

        public void OnNetworkInSession(Action<MLPNetwork?> networkChanged)
        {
            _activeSessionNetChanged = (sender, args) =>
            {
                if (args.PropertyName == nameof(Session.Network))
                {
                    networkChanged((sender as Session)!.Network);
                }
            };

            if (_appState.ActiveSession != null)
            {
                networkChanged(_appState.ActiveSession.Network);

                _appState.ActiveSession.PropertyChanged -= _activeSessionNetChanged;
                _appState.ActiveSession.PropertyChanged += _activeSessionNetChanged;
            }

            _netActiveSessionChanged = (sender, args) =>
            {
                networkChanged(args.next.Network);

                args.next.PropertyChanged -= _activeSessionNetChanged;
                args.next.PropertyChanged += _activeSessionNetChanged;
            };
            _appState.ActiveSessionChanged += _netActiveSessionChanged;
        }

        public void OnNetworkChanged(Action<MLPNetwork> networkChanged)
        {
            OnNetworkInSession(network =>
            {
                if (network != null) networkChanged(network);
            });
        }


        public void OnTrainingDataInSession(Action<TrainingData?> trainingDataChanged)
        {
            CoreOnTrainingDataInSession(trainingDataChanged, out _activeSessionDataInSession,
                out _dataActiveSessionInSession);
        }


        private void CoreOnTrainingDataInSession(Action<TrainingData?> trainingDataChanged,
            out PropertyChangedEventHandler activeSessionDataInSession,
            out EventHandler<(Session? prev, Session next)> dataActiveSessionInSession)
        {
            activeSessionDataInSession = (sender, args) =>
            {
                if (args.PropertyName == nameof(Session.TrainingData))
                {
                    trainingDataChanged((sender as Session)!.TrainingData);
                }
            };

            if (_appState.ActiveSession != null)
            {
                trainingDataChanged(_appState.ActiveSession.TrainingData);

                _appState.ActiveSession.PropertyChanged -= activeSessionDataInSession;
                _appState.ActiveSession.PropertyChanged += activeSessionDataInSession;
            }

            var activeSessionDataInSession_ = activeSessionDataInSession;
            dataActiveSessionInSession = (sender, args) =>
            {
                trainingDataChanged(args.next.TrainingData);

                args.next.PropertyChanged -= activeSessionDataInSession_;
                args.next.PropertyChanged += activeSessionDataInSession_;
            };
            _appState.ActiveSessionChanged += dataActiveSessionInSession;
        }

        public void OnTrainingDataChanged(Action<TrainingData> trainingDataChanged)
        {
            CoreOnTrainingDataInSession(data =>
            {
                if (data != null) trainingDataChanged(data);
            }, out _activeSessionDataChanged, out _dataActiveSessionChanged);
        }


        public void OnTrainingDataPropertyChanged(Action<TrainingData> trainingDataChanged,
            Func<string, bool> propertySelector)
        {
            CoreOnTrainingDataInSession(data =>
            {
                if (data == null) return;
                if (_dataPropertyChanged == null)
                {
                    _dataPropertyChanged = (sender, args) =>
                    {
                        if (propertySelector(args.PropertyName))
                        {
                            trainingDataChanged((sender as TrainingData)!);
                        }
                    };
                }
                else
                {
                    data.PropertyChanged -= _dataPropertyChanged;
                }

                data.PropertyChanged += _dataPropertyChanged;
            }, out _activeSessionDataPropertyChanged, out _dataActiveSessionPropertyChanged);
        }
    }
}