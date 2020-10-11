using System;
using System.Collections.Generic;
using Common.Domain;
using MathNet.Numerics.LinearAlgebra;
using NNLib;
using Prediction.Application.Controllers;
using Prism.Mvvm;

namespace Prediction.Application
{
    public class ModuleState : BindableBase
    {
        private readonly Dictionary<Session, PredictControllerMemento> _predictMementos = new Dictionary<Session, PredictControllerMemento>();
        private readonly AppState _appState;

        public ModuleState(AppState appState)
        {
            _appState = appState;
        }

        public PredictControllerMemento? GetSessionPredictMemento()
        {
            return _predictMementos.ContainsKey(_appState.ActiveSession!)
                ? _predictMementos[_appState.ActiveSession!]
                : null;
        }

        public void SetSessionPredictMemento(Session session, PredictControllerMemento memento)
        {
            _predictMementos[session] = memento;
        }
    }
}