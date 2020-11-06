using Data.Application;
using Data.Application.Interfaces;
using Data.Application.ViewModels;
using Data.Presentation.Services;
using Data.Presentation.Views;
using Prism.Ioc;

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