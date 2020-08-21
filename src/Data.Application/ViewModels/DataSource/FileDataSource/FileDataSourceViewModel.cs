using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Common.Framework;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.Normalization;
using Data.Application.ViewModels.DataSource.Preview;
using Infrastructure;

namespace Data.Application.ViewModels.DataSource.FileDataSource
{
    public class FileDataSourceViewModel : ViewModelBase<FileDataSourceViewModel>
    {
        private Visibility _showLoadingVisibility = Visibility.Visible;

        public FileDataSourceViewModel(IFileDataSourceService service,DataSourcePreviewViewModel dataSourcePreviewVm, NormalizationViewModel normalizationVm)
        {
            DataSourcePreviewVm = dataSourcePreviewVm;
            NormalizationVm = normalizationVm;
            KeepAlive = true;

            service.Initialized?.Invoke();
        }

        public Visibility ShowLoadingVisibility
        {
            get => _showLoadingVisibility;
            set => SetProperty(ref _showLoadingVisibility, value);
        }

        public DataSourcePreviewViewModel DataSourcePreviewVm { get; }
        public NormalizationViewModel NormalizationVm { get; }
    } 
}
