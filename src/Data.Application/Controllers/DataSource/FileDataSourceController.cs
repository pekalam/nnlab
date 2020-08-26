using Data.Application.Services;
using Data.Application.ViewModels.DataSetDivision;
using Data.Application.ViewModels.DataSource.FileDataSource;
using Data.Application.ViewModels.DataSource.VariablesSelection;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using System.Windows;
using Common.Domain;
using Common.Framework;
using Prism.Ioc;

namespace Data.Application.Controllers.DataSource
{
    public interface IFileDataSourceController : ISingletonController
    {
        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IFileDataSourceController, FileDataSourceController>();

        }
    }

    public class FileDataSourceController : IFileDataSourceController
    {
        private IRegionManager _rm;
        private IEventAggregator _ea;
        private AppState _appState;

        public FileDataSourceController(IFileDataSourceService service, IRegionManager rm, IEventAggregator ea, AppState appState)
        {
            _rm = rm;
            _ea = ea;
            _appState = appState;


            service.SelectVariablesCommand = new DelegateCommand(SelectVariables);
            service.DivideDatasetCommand = new DelegateCommand(DivideDataset);

            service.Initialized += () =>
            {
                var vm = FileDataSourceViewModel.Instance;

                vm.DataSourcePreviewVm.Loaded += () => vm.ShowLoadingVisibility = Visibility.Collapsed;

                //TODO
                //_actionMenuNavService.SetLeftMenu<ActionMenuLeftView>();
            };
        }

        private void DivideDataset()
        {
            var session = _appState.ActiveSession;
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Divide data set"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate("DataSetDivisionView", new FileDataSetDivisionNavParams(session.SingleDataFile));
        }

        private void SelectVariables()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Select variables"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate("VariablesSelectionView");
        }

        public void Initialize()
        {
            
        }
    }
}
