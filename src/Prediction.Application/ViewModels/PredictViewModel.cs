using System;
using System.Collections.Generic;
using System.Text;
using Common.Domain;
using Common.Framework;
using NNLibAdapter;
using Prediction.Application.Services;
using Prism.Regions;
using SharedUI.MatrixPreview;

namespace Prediction.Application.ViewModels
{
    public class PredictViewModel : ViewModelBase<PredictViewModel>
    {
        private NNLibModelAdapter _modelAdapter;

        public PredictViewModel()
        {
            
        }

        public PredictViewModel(IPredictService service)
        {
            Service = service;
        }

        public NNLibModelAdapter ModelAdapter
        {
            get => _modelAdapter;
            set => SetProperty(ref _modelAdapter, value);
        }

        public IPredictService Service { get; }

        public MatrixPreviewViewModel InputMatrixVm { get; set; } = new MatrixPreviewViewModel();
        public MatrixPreviewViewModel OutputMatrixVm { get; set; } = new MatrixPreviewViewModel();

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}
