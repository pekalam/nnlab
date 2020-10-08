using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Common.Logging;
using NNLib;
using Prism.Mvvm;

namespace Common.Domain
{
    [Flags]
    public enum DuplicateOptions
    {
        All = 1, NoData = 2, NoNetwork = 4, NoTrainingParams = 8
    }

    public class AppStateHelper
    {
        private AppState _appState;

        private EventHandler<(Session? prev, Session next)> _netActiveSessionChanged = null!;
        private PropertyChangedEventHandler _activeSessionNetChanged = null!;

        private EventHandler<(Session? prev, Session next)> _dataActiveSessionInSession = null!;
        private PropertyChangedEventHandler _activeSessionDataInSession = null!;
        private EventHandler<(Session? prev, Session next)> _dataActiveSessionChanged = null!;
        private PropertyChangedEventHandler _activeSessionDataChanged = null!;
        private EventHandler<(Session? prev, Session next)> _dataActiveSessionPropertyChanged = null!;
        private PropertyChangedEventHandler _activeSessionDataPropertyChanged = null!;

        private PropertyChangedEventHandler _dataPropertyChanged = null!;

        public AppStateHelper(AppState appState)
        {
            _appState = appState;
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
            CoreOnTrainingDataInSession(trainingDataChanged, out _activeSessionDataInSession, out _dataActiveSessionInSession);
        }


        private void CoreOnTrainingDataInSession(Action<TrainingData?> trainingDataChanged,out PropertyChangedEventHandler activeSessionDataInSession,
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


        public void OnTrainingDataPropertyChanged(Action<TrainingData> trainingDataChanged, Func<string,bool> propertySelector)
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

    public class AppState : BindableBase
    {
        private Session? _activeSession;

        public event EventHandler<(Session? prev,Session next)>? ActiveSessionChanged;
        public event EventHandler<Session>? SessionCreated; 

        public Session? ActiveSession
        {
            get => _activeSession;
            set
            {
                if (value == null) throw new NullReferenceException("Null session");
                if (!Sessions.Contains(value)) throw new ArgumentException("Session not found");
                Log.Debug("Active session changed to " + value.Name);
                var tempPrevious = _activeSession;
                SetProperty(ref _activeSession, value);
                ActiveSessionChanged?.Invoke(this, (tempPrevious, value));
            }
        }

        public ObservableCollection<Session> Sessions { get; } = new ObservableCollection<Session>();

        public Session CreateSession(string name = "")
        {
            if (name == "")
            {
                name = "Unnamed" + (Sessions.Count > 0 ? " " + Sessions.Count : "");
            }
            var newSession = new Session(name);
            Sessions.Add(newSession);
            Log.Debug($"Session {newSession.Name} created");
            SessionCreated?.Invoke(this, newSession);

            if (Sessions.Count == 1)
            {
                ActiveSession = newSession;
            }

            return newSession;
        }

        public Session DuplicateActiveSession(DuplicateOptions duplicateOptions = DuplicateOptions.All)
        {
            if (_activeSession == null) throw new InvalidOperationException("Cannot duplicate - null active session");

            var cpy = _activeSession.CloneWithName("Unnamed" + (Sessions.Count > 0 ? " " + Sessions.Count : ""), duplicateOptions);
            Log.Debug($"Session {_activeSession!.Name} duplicated. Duplicated name: {cpy.Name}");

            Sessions.Add(cpy);
            ActiveSession = cpy;
            return cpy;
        }
    }


    public class Session : BindableBase
    {
        private TrainingData? _trainingData;
        private MLPNetwork? _network;
        private MLPNetwork? _initialNetwork;
        private TrainingParameters? _trainingParameters;
        private string? _singleDataFile;
        private string? _trainingDataFile;
        private string? _validationDataFile;
        private string? _testDataFile;
        private string _name;

        public event Action<MLPNetwork>? NetworkStructureChanged;
        public event Action<MLPNetwork>? NetworkParametersChanged;

        public Session(string name)
        {
            Name = name;
        }

        private Session(string name, TrainingData? trainingData, MLPNetwork? network, TrainingParameters? trainingParameters, string? singleDataFile, string? trainingDataFile, string? validationDataFile, string? testDataFile)
        {
            Name = name;
            _trainingData = trainingData;
            _network = network;
            _trainingParameters = trainingParameters;
            _singleDataFile = singleDataFile;
            _trainingDataFile = trainingDataFile;
            _validationDataFile = validationDataFile;
            _testDataFile = testDataFile;
        }

        internal Session CloneWithName(string name, DuplicateOptions opt)
        {
            return new Session(name, 
                !opt.HasFlag(DuplicateOptions.NoData) ? TrainingData?.Clone() : null,
                !opt.HasFlag(DuplicateOptions.NoNetwork) ? Network?.Clone() : null,
                !opt.HasFlag(DuplicateOptions.NoTrainingParams) ? TrainingParameters?.Clone() : null,
                SingleDataFile,TrainingDataFile, ValidationDataFile, TestDataFile
                );
        }

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("Name cannot be null or contain whitespace characters");
                }
                SetProperty(ref _name, value);
            }
        }

        public string? SingleDataFile
        {
            get => _singleDataFile;
            set => SetProperty(ref _singleDataFile, value);
        }

        public string? TrainingDataFile
        {
            get => _trainingDataFile;
            set => SetProperty(ref _trainingDataFile, value);
        }

        public string? ValidationDataFile
        {
            get => _validationDataFile;
            set => SetProperty(ref _validationDataFile, value);
        }

        public string? TestDataFile
        {
            get => _testDataFile;
            set => SetProperty(ref _testDataFile, value);
        }

        public TrainingData? TrainingData
        {
            get => _trainingData;
            set
            {
                SetProperty(ref _trainingData, value);
                if (value == null || _network == null) return;
                if (TrainingParameters == null) TrainingParameters = new TrainingParameters();
            }
        }

        public void UnloadTrainingData()
        {
            if(_trainingData == null) throw new NullReferenceException("Null trainingData");

            _trainingData.Sets.TrainingSet.Dispose();
            _trainingData.Sets.ValidationSet?.Dispose();
            _trainingData.Sets.TestSet?.Dispose();

            _trainingData = null;
        }

        public MLPNetwork? Network
        {
            get => _network;
            set
            {
                SetProperty(ref _network, value);
                if (value == null || _trainingData == null) return;
                _initialNetwork = value.Clone();
                if (TrainingParameters == null) TrainingParameters = new TrainingParameters();
            }
        }

        public void RaiseNetworkStructureChanged()
        {
            Debug.Assert(Network != null);
            _initialNetwork = Network.Clone();
            NetworkStructureChanged?.Invoke(Network);
        }

        public void RaiseNetworkParametersChanged()
        {
            Debug.Assert(Network != null);
            _initialNetwork = Network.Clone();
            NetworkParametersChanged?.Invoke(Network);
        }

        public void ResetNetworkToInitial()
        {
            Network = _initialNetwork;
            TrainingReports = new TrainingReportsCollection();
        }

        public TrainingReportsCollection TrainingReports { get; private set; } = new TrainingReportsCollection();

        public TrainingParameters? TrainingParameters
        {
            get => _trainingParameters;
            set => SetProperty(ref _trainingParameters, value);
        }
    }
}
