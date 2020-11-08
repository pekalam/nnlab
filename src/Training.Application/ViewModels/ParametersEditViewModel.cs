using Common.Domain;
using Common.Framework;

namespace Training.Application.ViewModels
{
    public class ParametersEditViewModel : ViewModelBase<ParametersEditViewModel>
    {
#pragma warning disable 8618
        public ParametersEditViewModel()
#pragma warning restore 8618
        {
            
        }

        public ParametersEditViewModel(AppState appState)
        {
            AppState = appState;
        }

        public AppState AppState { get; }
    }
}
