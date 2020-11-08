using Common.Logging;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace Common.Domain
{
    public class AppState : BindableBase
    {
        private Session? _activeSession;

        public event EventHandler<(Session? prev, Session next)>? ActiveSessionChanged;
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

            var cpy = _activeSession.CloneWithName("Unnamed" + (Sessions.Count > 0 ? " " + Sessions.Count : ""),
                duplicateOptions);
            Log.Debug($"Session {_activeSession!.Name} duplicated. Duplicated name: {cpy.Name}");

            Sessions.Add(cpy);
            ActiveSession = cpy;
            return cpy;
        }
    }
}