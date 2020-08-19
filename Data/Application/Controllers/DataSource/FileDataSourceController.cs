using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Data.Application.Services;
using Data.Application.ViewModels.DataSetDivision;
using Data.Application.ViewModels.DataSource.FileDataSource;
using Data.Presentation.Views.DataSetDivision;
using Data.Presentation.Views.DataSource.FileDataSource;
using Data.Presentation.Views.DataSource.VariablesSelection;
using Infrastructure;
using Infrastructure.Messaging;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace Data.Application.Controllers.DataSource
{
    public class FileDataSourceController
    {
        private IRegionManager _rm;
        private IEventAggregator _ea;
        private readonly IActionMenuNavigationService _actionMenuNavService;

        public FileDataSourceController(IFileDataSourceService service, IRegionManager rm, IEventAggregator ea, IActionMenuNavigationService actionMenuNavService)
        {
            _rm = rm;
            _ea = ea;
            _actionMenuNavService = actionMenuNavService;


            service.SelectVariablesCommand = new DelegateCommand(SelectVariables);
            service.DivideDatasetCommand = new DelegateCommand(DivideDataset);

            service.Initialized += () =>
            {
                var vm = FileDataSourceViewModel.Instance;

                vm.DataSourcePreviewVm.Loaded += () => vm.ShowLoadingVisibility = Visibility.Collapsed;

                _actionMenuNavService.SetLeftMenu<ActionMenuLeftView>();
            };
        }

        private void DivideDataset()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Divide data set"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate(nameof(DataSetDivisionView), new FileDataSetDivisionNavParams("C:\\Users\\Marek Pękala\\Desktop\\bigsin.csv"));
        }

        private void SelectVariables()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Select variables"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate(nameof(VariablesSelectionView));
        }
    }
}
