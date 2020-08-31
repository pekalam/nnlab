﻿using System;
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

        
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public TrainingViewModel()
        {
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
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
