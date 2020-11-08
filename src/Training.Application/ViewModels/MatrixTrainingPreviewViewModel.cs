using Common.Framework;
using Prism.Regions;
using SharedUI.MatrixPreview;
using Training.Application.Controllers;

namespace Training.Application.ViewModels
{
    class MatrixTrainingPreviewViewModel : ViewModelBase<MatrixTrainingPreviewViewModel>
    {
        public MatrixTrainingPreviewViewModel(IMatrixTrainingPreviewController service)
        {
            Service = service;
            service.Initialize(this);
        }

        public IMatrixTrainingPreviewController Service { get; }

        public MatrixPreviewViewModel MatVm { get; set; } = new MatrixPreviewViewModel();

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);
        }
    }
}
