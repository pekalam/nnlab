using System;
using Common.Framework;
using NNLibAdapter;
using Training.Domain;

namespace Training.Application.ViewModels
{
    public class TrainingNetworkPreviewViewModel : ViewModelBase<TrainingNetworkPreviewViewModel>
    {
        private NNLibModelAdapter _modelAdapter;

        public TrainingNetworkPreviewViewModel(ModuleState moduleState)
        {
            if (moduleState.ActiveSession?.Network != null)
            {
                _modelAdapter = new NNLibModelAdapter();
                _modelAdapter.SetNeuralNetwork(moduleState.ActiveSession.Network);
            }
            else
            {
                throw new Exception();
            }
        }

        public NNLibModelAdapter ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }


        public void StopAnimation(bool resetColors) => ModelAdapter.ColorAnimation.StopAnimation(resetColors);
    }
}