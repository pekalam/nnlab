using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Data.Application.ViewModels.DataSource.Preview;
using Infrastructure;

namespace Data.Application.ViewModels.DataSource.FileDataSource
{
    public class FileDataSourceViewModel : ViewModelBase<FileDataSourceViewModel>
    {
        private Visibility _showLoadingVisibility = Visibility.Visible;

        public FileDataSourceViewModel(DataSourcePreviewViewModel dataSourcePreviewVm)
        {
            DataSourcePreviewVm = dataSourcePreviewVm;
            KeepAlive = true;
        }

        public Visibility ShowLoadingVisibility
        {
            get => _showLoadingVisibility;
            set => SetProperty(ref _showLoadingVisibility, value);
        }

        public DataSourcePreviewViewModel DataSourcePreviewVm { get; }
    } 
}
