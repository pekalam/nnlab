using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Common.Logging;
using NNLib;
using Prism.Mvvm;

namespace Common.Domain
{
    [Flags]
    public enum DuplicateOptions
    {
        All, NoData, NoNetwork, NoTrainingParams
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

        public Session CreateSession(string? name = "")
        {
            if (name == "")
            {
                name = "Unnamed" + (Sessions.Count > 0 ? " " + Sessions.Count : string.Empty);
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
            if (ActiveSession == null) throw new InvalidOperationException("Cannot duplicate - null active session");

            var cpy = ActiveSession.CloneWithName("Unnamed" + (Sessions.Count > 0 ? " " + Sessions.Count : string.Empty), duplicateOptions);
            Log.Debug($"Session {_activeSession.Name} duplicated. Duplicated name: {cpy.Name}");

            Sessions.Add(cpy);
            ActiveSession = cpy;
            return cpy;
        }
    }


    public class Session : BindableBase
    {
        private TrainingData? _trainingData;
        private MLPNetwork? _network;
        private TrainingParameters? _trainingParameters;
        private string? _singleDataFile;
        private string? _trainingDataFile;
        private string? _validationDataFile;
        private string? _testDataFile;

        public event Action<MLPNetwork>? NetworkStructureChanged;

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

        public string Name { get; }

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
                TrainingParameters = new TrainingParameters();
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
                TrainingParameters = new TrainingParameters();
            }
        }

        public void RaiseNetworkStructureChanged()
        {
            NetworkStructureChanged?.Invoke(Network);
        }

        public TrainingReportsCollection TrainingReports { get; } = new TrainingReportsCollection();

        public TrainingParameters? TrainingParameters
        {
            get => _trainingParameters;
            set => SetProperty(ref _trainingParameters, value);
        }
    }
}
