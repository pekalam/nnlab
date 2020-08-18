using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Data.Application.Services;
using Data.Application.ViewModels.DataSource.FileDataSource;
using Data.Presentation.Views.DataSource.FileDataSource;
using Data.Presentation.Views.DataSource.Preview;
using Prism.Regions;

namespace Data.Application.Controllers.DataSource
{
    public class FileDataSourceController
    {
        public FileDataSourceController(IFileDataSourceService service)
        {
            service.Initialized += () =>
            {
                var vm = FileDataSourceViewModel.Instance;

                vm.DataSourcePreviewVm.Loaded += () => vm.ShowLoadingVisibility = Visibility.Collapsed;
            };
        }
    }
}
