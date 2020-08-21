using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NNLib;
using Prism.Mvvm;

namespace Common.Domain
{
    public enum DuplicateOptions
    {
        All, NoData, NoNetwork, NoTrainingParams
    }


    public class AppState : BindableBase
    {
        private Session? _activeSession;


        public Session? ActiveSession
        {
            get => _activeSession;
            set
            {
                if (value == null) throw new NullReferenceException("Null session");
                if (!Sessions.Contains(value)) throw new ArgumentException("Session not found");
                SetProperty(ref _activeSession, value);
            }
        }

        public ObservableCollection<Session> Sessions { get; } = new ObservableCollection<Session>();

        public Session CreateSession()
        {
            var newSession = new Session("Unnamed" + (Sessions.Count > 0 ? " " + Sessions.Count : string.Empty));
            Sessions.Add(newSession);

            if (Sessions.Count == 1)
            {
                ActiveSession = newSession;
            }

            return newSession;
        }

        public Session DuplicateActiveSession(DuplicateOptions duplicateOptions = DuplicateOptions.All)
        {
            if (ActiveSession == null) throw new InvalidOperationException("Cannot duplicate - null active session");

            var cpy = new Session("Unnamed" + (Sessions.Count > 0 ? " " + Sessions.Count : string.Empty));

            cpy.Network = duplicateOptions != DuplicateOptions.NoNetwork ? ActiveSession.Network?.Clone() : null;
            cpy.TrainingData = duplicateOptions != DuplicateOptions.NoData ? ActiveSession.TrainingData?.Clone() : null;
            cpy.TrainingParameters = duplicateOptions != DuplicateOptions.NoTrainingParams
                ? ActiveSession.TrainingParameters?.Clone()
                : null;


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

        public Session(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string? SingleDataFile { get; set; }
        public string? TrainingDataFile { get; set; }
        public string? ValidationDataFile { get; set; }
        public string? TestDataFile { get; set; }


        public TrainingData? TrainingData
        {
            get => _trainingData;
            set => SetProperty(ref _trainingData, value);
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
            set => SetProperty(ref _network, value);
        }

        public TrainingReportsCollection TrainingReports { get; } = new TrainingReportsCollection();

        public TrainingParameters? TrainingParameters
        {
            get => _trainingParameters;
            set => SetProperty(ref _trainingParameters, value);
        }
    }
}
