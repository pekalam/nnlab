﻿using Common.Framework;
using NNLibAdapter;
using Prism.Regions;
using Training.Application.Controllers;

namespace Training.Application.ViewModels
{
    public class TrainingNetworkPreviewViewModel : ViewModelBase<TrainingNetworkPreviewViewModel>
    {
        private NNLibModelAdapter? _modelAdapter;

        public TrainingNetworkPreviewViewModel(ModuleState moduleState, ITrainingNetworkPreviewController service)
        {
            Service = service;
            service.Initialize(this);
        }

        public ITrainingNetworkPreviewController Service { get; }

        public NNLibModelAdapter? ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}