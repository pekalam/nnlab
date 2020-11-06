using System;
using System.Diagnostics;
using Data.Application.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using System.Windows;
using Common.Domain;
using Common.Framework;
using Data.Application.ViewModels;
using Prism.Ioc;

namespace Data.Application.Controllers.DataSource
{
    public interface IFileDataSourceController : ITransientController
    {
        Action Initialized { get; set; }
        DelegateCommand SelectVariablesCommand { get; set; }
        DelegateCommand DivideDatasetCommand { get; set; }
        Action<NavigationContext> Navigated { get; }

        public static void Register(IContainerRegistry cr)
        {
            cr.RegisterSingleton<IFileDataSourceController, FileDataSourceController>();
        }
    }

    internal class FileDataSourceController : ControllerBase<FileDataSourceViewModel>,IFileDataSourceController
    {
        private IRegionManager _rm;
        private IEventAggregator _ea;
        private AppState _appState;

        public FileDataSourceController(IRegionManager rm, IEventAggregator ea,
            AppState appState)
        {
            _rm = rm;
            _ea = ea;
            _appState = appState;


            SelectVariablesCommand = new DelegateCommand(SelectVariables);
            DivideDatasetCommand = new DelegateCommand(DivideDataset);

            Navigated = NavigatedAction;

            Initialized += () =>
            {
                var vm = FileDataSourceViewModel.Instance!;

                vm.DataSourcePreviewVm.Loaded += () => vm.ShowLoadingVisibility = Visibility.Collapsed;
            };
        }

        private void NavigatedAction(NavigationContext ctx)
        {
            var vm = FileDataSourceViewModel.Instance!;

            ctx.Parameters.TryGetValue("Multi", out bool multiFile);
            vm.IsDivideDataSetEnabled = !multiFile;
        }

        private void DivideDataset()
        {
            var session = _appState.ActiveSession!;
            Debug.Assert(session.SingleDataFile != null);
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Divide data set"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate("DataSetDivisionView",
                new FileDataSetDivisionNavParams(session.SingleDataFile));
        }

        private void SelectVariables()
        {
            _ea.GetEvent<ShowFlyout>().Publish(new FlyoutArgs()
            {
                Title = "Select variables"
            });
            _rm.Regions[AppRegions.FlyoutRegion].RequestNavigate("VariablesSelectionView");
        }

        public Action Initialized { get; set; }
        public DelegateCommand SelectVariablesCommand { get; set; }
        public DelegateCommand DivideDatasetCommand { get; set; }
        public Action<NavigationContext> Navigated { get; }
    }
}