using System.Collections.Generic;
using System.Diagnostics;
using Common.Domain;
using NNLib.Common;

namespace Data.Domain
{
    public class ModuleState
    {
        private readonly Dictionary<Session, SupervisedTrainingSets> _orgSets = new Dictionary<Session, SupervisedTrainingSets>();
        private readonly AppState _appState;

        public ModuleState(AppState appState)
        {
            _appState = appState;
        }



        public void StoreOriginalSets()
        {
            Debug.Assert(_appState.ActiveSession?.TrainingData != null);
            _orgSets[_appState.ActiveSession] = _appState.ActiveSession.TrainingData.Sets;
        }

        public SupervisedTrainingSets? OriginalSets
        {
            get
            {
                _orgSets.TryGetValue(_appState.ActiveSession!, out var data);
                return data;
            }
        }
    }
}
