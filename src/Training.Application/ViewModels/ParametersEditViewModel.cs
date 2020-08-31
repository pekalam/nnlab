using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Common.Framework;

namespace Training.Application.ViewModels
{
    public class ParametersEditViewModel : ViewModelBase<ParametersEditViewModel>
    {
        public ParametersEditViewModel()
        {
            
        }

        public ParametersEditViewModel(AppState appState)
        {
            AppState = appState;
        }

        public AppState AppState { get; }
    }
}
