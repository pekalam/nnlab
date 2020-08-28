using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.Normalization;
using Data.Application.ViewModels.DataSource.Preview;
using System.Windows;
using Prism.Regions;

namespace Data.Application.ViewModels.DataSource.FileDataSource
{
    public class FileDataSourceViewModel : ViewModelBase<FileDataSourceViewModel>
    {
        private Visibility _showLoadingVisibility = Visibility.Visible;
        private bool _isDivideDataSetEnabled;

        public FileDataSourceViewModel(IFileDataSourceService service,DataSourcePreviewViewModel dataSourcePreviewVm, NormalizationViewModel normalizationVm)
        {
            Service = service;
            DataSourcePreviewVm = dataSourcePreviewVm;
            NormalizationVm = normalizationVm;
            KeepAlive = true;

            service.Initialized?.Invoke();
        }

        public IFileDataSourceService Service { get; }

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
