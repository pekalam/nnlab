using Data.Application;
using Data.Application.Interfaces;
using Data.Application.ViewModels;
using Data.Application.ViewModels.CustomDataSet;
using Data.Application.ViewModels.DataSetDivision;
using Data.Application.ViewModels.DataSource.FileDataSource;
using Data.Application.ViewModels.DataSource.Normalization;
using Data.Application.ViewModels.DataSource.Preview;
using Data.Application.ViewModels.DataSource.VariablesSelection;
using Data.Application.ViewModels.DataSourceSelection;
using Data.Presentation.Services;
using Data.Presentation.Views;
using Data.Presentation.Views.CustomDataSet;
using Data.Presentation.Views.DataSetDivision;
using Data.Presentation.Views.DataSource.FileDataSource;
using Data.Presentation.Views.DataSource.Normalization;
using Data.Presentation.Views.DataSource.Preview;
using Data.Presentation.Views.DataSource.VariablesSelection;
using Data.Presentation.Views.DataSourceSelection;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace Data.Presentation
{
    public static class Bootstraper
    {
        public static void RegisterTypes(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IFileDialogService, FileDialogService>();

            cr.RegisterForNavigation<MultiFileSourceView>();
            cr.RegisterForNavigation<SelectDataSourceView>();
            cr.RegisterForNavigation<SingleFileSourceView>();
            cr.RegisterForNavigation<MultiFileSourceView>();
            cr.RegisterForNavigation<CustomDataSetView>();
            cr.RegisterForNavigation<DataSetDivisionView>();
            cr.RegisterForNavigation<DataSourcePreviewView>();
            cr.RegisterForNavigation<FileDataSourceView>();
            cr.RegisterForNavigation<NormalizationView>();
            cr.RegisterForNavigation<VariablesSelectionView>();
        }
    }
}