using System;
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

        public void DuplicateActive(DuplicateOptions duplicateOptions = DuplicateOptions.All)
        {
            var cpy = new Session("Unnamed" + (_sessions.Count > 0 ? " " + _sessions.Count : String.Empty));

            cpy.Network = duplicateOptions != DuplicateOptions.NoNetwork ? ActiveSession.Network : null;
            cpy.TrainingData = duplicateOptions != DuplicateOptions.NoData ? ActiveSession.TrainingData : null;
            cpy.TrainingParameters = duplicateOptions != DuplicateOptions.NoTrainingParams
                ? ActiveSession.TrainingParameters
                : null;


            _sessions.Add(cpy);
            SetActive(cpy);
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

    public class Session
    {
        
        public Session(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public TrainingData? TrainingData { get; set; }
        public MLPNetwork? Network { get; set; }
        public TrainingSessionReport? TrainingReport { get; set; }
        public TrainingParameters? TrainingParameters { get; set; }
    }
}
