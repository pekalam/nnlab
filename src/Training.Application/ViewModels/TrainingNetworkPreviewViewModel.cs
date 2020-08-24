using Common.Framework;
using NNLibAdapter;

namespace Training.Application.ViewModels
{
    public class TrainingNetworkPreviewViewModel : ViewModelBase<TrainingNetworkPreviewViewModel>
    {
        private NNLibModelAdapter _modelAdapter = new NNLibModelAdapter();

        public NNLibModelAdapter ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }


        public void StopAnimation(bool resetColors) => ModelAdapter.ColorAnimation.StopAnimation(resetColors);
    }
}