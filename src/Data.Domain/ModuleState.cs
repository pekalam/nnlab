﻿using System.Collections.Generic;
using Common.Domain;

namespace Data.Domain
{
    internal class ModuleState
    {
        private readonly Dictionary<Session, TrainingData> _orgTrainingData = new Dictionary<Session, TrainingData>();
        private readonly AppState _appState;

        public ModuleState(AppState appState)
        {
            _appState = appState;
        }


        public void StoreOriginalTrainingData()
        {
            _orgTrainingData[_appState.ActiveSession] = _appState.ActiveSession.TrainingData;
        }

        public TrainingData? OriginalTrainingData
        {
            get
            {
                _orgTrainingData.TryGetValue(_appState.ActiveSession, out var data);
                return data;
            }
        }
    }
}
