using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Common.Framework;
using Prism.Regions;
using Training.Application.Services;
using Training.Domain;
using Unity;

namespace Training.Application.ViewModels
{
    public class TrainingViewModel : ViewModelBase<TrainingViewModel>
    {
        private Visibility _selectPanelsButtonVisibility = Visibility.Visible;
        private Visibility _panelsContainerVisibility = Visibility.Hidden;
        private Visibility _upperSelectPanelsButtonVisibility = Visibility.Collapsed;

        public TrainingViewModel()
        {
        }

        [InjectionConstructor]
        public TrainingViewModel(ITrainingService service, ModuleState moduleState)
        {
            Service = service;
            ModuleState = moduleState;
            KeepAlive = true;
        }

        public ModuleState ModuleState { get; }

        public ITrainingService Service { get; }

        public Visibility SelectPanelsButtonVisibility
        {
            get => _selectPanelsButtonVisibility;
            set => SetProperty(ref _selectPanelsButtonVisibility, value);
        }

        public Visibility PanelsContainerVisibility
        {
            get => _panelsContainerVisibility;
            set => SetProperty(ref _panelsContainerVisibility, value);
        }

        public Visibility UpperSelectPanelsButtonVisibility
        {
            get => _upperSelectPanelsButtonVisibility;
            set => SetProperty(ref _upperSelectPanelsButtonVisibility, value);
        }
    }
}
