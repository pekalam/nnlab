using System.Collections.Generic;
using Approximation.Application.Controllers;
using Common.Domain;
using Prism.Mvvm;

namespace Approximation.Application
{
    public class ModuleState : BindableBase
    {
        private readonly Dictionary<Session, ApproximationControllerMemento> _predictMementos = new Dictionary<Session, ApproximationControllerMemento>();
        private readonly AppState _appState;

        public ModuleState(AppState appState)
        {
            _appState = appState;
        }

        public ApproximationControllerMemento? GetSessionPredictMemento()
        {
            return _predictMementos.ContainsKey(_appState.ActiveSession!)
                ? _predictMementos[_appState.ActiveSession!]
                : null;
        }

        public void SetSessionPredictMemento(Session session, ApproximationControllerMemento memento)
        {
            _predictMementos[session] = memento;
        }
    }
}