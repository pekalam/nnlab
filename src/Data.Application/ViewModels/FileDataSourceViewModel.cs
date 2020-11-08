using Common.Framework;
using Data.Application.Controllers.DataSource;
using Prism.Regions;
using System.Windows;

namespace Data.Application.ViewModels
{
    public class FileDataSourceViewModel : ViewModelBase<FileDataSourceViewModel>
    {
        private Visibility _showLoadingVisibility = Visibility.Visible;
        private bool _isDivideDataSetEnabled;

        public FileDataSourceViewModel(IFileDataSourceController service,DataSourcePreviewViewModel dataSourcePreviewVm, NormalizationViewModel normalizationVm)
        {
            Service = service;
            DataSourcePreviewVm = dataSourcePreviewVm;
            NormalizationVm = normalizationVm;
            KeepAlive = true;

            service.Initialize(this);
            service.Initialized?.Invoke();
        }

        public IFileDataSourceController Service { get; }

        public Visibility ShowLoadingVisibility
        {
            get => _showLoadingVisibility;
            set => SetProperty(ref _showLoadingVisibility, value);
        }

        public bool IsDivideDataSetEnabled
        {
            get => _isDivideDataSetEnabled;
            set => SetProperty(ref _isDivideDataSetEnabled, value);
        }

        public DataSourcePreviewViewModel DataSourcePreviewVm { get; }
        public NormalizationViewModel NormalizationVm { get; }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Service.Navigated(navigationContext);

        }
    } 
}
