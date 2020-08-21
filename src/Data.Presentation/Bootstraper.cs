using Data.Application;
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

            cr.RegisterForNavigation<MultiFileSourceView>(nameof(MultiFileSourceViewModel));
            cr.RegisterForNavigation<SelectDataSourceView>(nameof(SelectDataSourceViewModel));
            cr.RegisterForNavigation<SingleFileSourceView>(nameof(SingleFileSourceViewModel));
            cr.RegisterForNavigation<MultiFileSourceView>(nameof(MultiFileSourceViewModel));
            cr.RegisterForNavigation<CustomDataSetView>(nameof(CustomDataSetViewModel));
            cr.RegisterForNavigation<DataSetDivisionView>(nameof(DataSetDivisionViewModel));
            cr.RegisterForNavigation<DataSourcePreviewView>(nameof(DataSourcePreviewViewModel));
            cr.RegisterForNavigation<FileDataSourceView>(nameof(FileDataSourceViewModel));
            cr.RegisterForNavigation<NormalizationView>(nameof(NormalizationViewModel));
            cr.RegisterForNavigation<VariablesSelectionView>(nameof(VariablesSelectionViewModel));
        }
    }
}