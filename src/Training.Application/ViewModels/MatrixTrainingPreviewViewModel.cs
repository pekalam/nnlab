using System;
using System.Collections.Generic;
using System.Text;
using Common.Framework;
using Prism.Regions;
using SharedUI.MatrixPreview;
using Training.Application.Services;

namespace Training.Application.ViewModels
{
    class MatrixTrainingPreviewViewModel : ViewModelBase<MatrixTrainingPreviewViewModel>
    {
        public MatrixTrainingPreviewViewModel(IMatrixTrainingPreviewService service)
        {
            Service = service;
            service.Initialize(this);
        }

        public IMatrixTrainingPreviewService Service { get; }

        public MatrixPreviewViewModel MatVm { get; set; } = new MatrixPreviewViewModel();

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}
