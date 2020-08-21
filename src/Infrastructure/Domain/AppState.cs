using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Infrastructure.Annotations;
using NNLib;
using Prism.Events;
using Prism.Mvvm;

namespace Infrastructure.Domain
{
    public class AppState : INotifyPropertyChanged
    {
        public AppState()
        {
            SessionManager = new SessionManager();
        }

        public SessionManager SessionManager { get; }








        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class SessionManager : INotifyPropertyChanged
    {
        public enum DuplicateOptions
        {
            All,NoData,NoNetwork,NoTrainingParams
        }


        private readonly List<Session> _sessions = new List<Session>();

        public Session? ActiveSession { get; private set; }

        public IReadOnlyList<Session> Sessions => _sessions;

        public Session Create()
        {
            var newSession = new Session("Unnamed" + (_sessions.Count > 0 ? " " + _sessions.Count : String.Empty));
            _sessions.Add(newSession);

            if (_sessions.Count == 1)
            {
                SetActive(newSession);
            }

            return newSession;
        }

        public Session DuplicateActive(DuplicateOptions duplicateOptions = DuplicateOptions.All)
        {
            if(ActiveSession == null) throw new InvalidOperationException("Cannot duplicate - null active session");

            var cpy = new Session("Unnamed" + (_sessions.Count > 0 ? " " + _sessions.Count : String.Empty));

            cpy.Network = duplicateOptions != DuplicateOptions.NoNetwork ? ActiveSession.Network?.Clone() : null;
            cpy.TrainingData = duplicateOptions != DuplicateOptions.NoData ? ActiveSession.TrainingData?.Clone() : null;
            cpy.TrainingParameters = duplicateOptions != DuplicateOptions.NoTrainingParams
                ? ActiveSession.TrainingParameters.Clone()
                : null;


            _sessions.Add(cpy);
            SetActive(cpy);
            return cpy;
        }

        public void SetActive(Session session)
        {
            ActiveSession = session;
            OnPropertyChanged(nameof(ActiveSession));
        }





        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Session : BindableBase
    {
        private TrainingData? _trainingData;
        private MLPNetwork? _network;
        private TrainingSessionReport? _trainingReport;
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

        public MLPNetwork? Network
        {
            get => _network;
            set => SetProperty(ref _network, value);
        }

        public TrainingSessionReport? TrainingReport
        {
            get => _trainingReport;
            set => SetProperty(ref _trainingReport, value);
        }

        public TrainingParameters? TrainingParameters
        {
            get => _trainingParameters;
            set => SetProperty(ref _trainingParameters, value);
        }
    }
}
