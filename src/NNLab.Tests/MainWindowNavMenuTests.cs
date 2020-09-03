using Common.Domain;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Regions;
using Shell.Application.PrismDecorators;
using Shell.Application.ViewModels;
using Shell.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TestUtils;
using Xunit;

namespace Shell.Application.Tests
{
    public static class RegionManagerTestUtils2
    {

        public static (Mock<RegionManagerNavigationDecorator> regionManager, Dictionary<string, Mock<IRegion>> regions, TestEa ea)
            CreateDecoratedTestRegionManager(AutoMocker mocker)
        {
            var ea = mocker.UseTestEa();
            var rm = new Mock<IRegionManager>();
            var regions = new Dictionary<string, Mock<IRegion>>();

            RegionManagerTestUtils.SetupEvents(rm, regions);

            var decrm = new Mock<RegionManagerNavigationDecorator>(() => new RegionManagerNavigationDecorator(rm.Object, ea));

            return (decrm, regions, ea);
        }

    }

    public static class AutoMockerExtensions2
    {
        public static (Mock<RegionManagerNavigationDecorator> rm, Dictionary<string, Mock<IRegion>> regions, TestEa ea) UseDecoratedTestRm(this AutoMocker mocker)
        {
            var (rm, regions, ea) = RegionManagerTestUtils2.CreateDecoratedTestRegionManager(mocker);

            mocker.Use(rm.Object);

            return (rm, regions, ea);
        }
    }


    public class MainWindowNavMenuTests
    {
        private AutoMocker _mocker = new AutoMocker();
        private AppState _appState;
        private TestEa _ea;
        private RegionManagerNavigationDecorator _rm;
        private MainWindowViewModel _vm;

        public MainWindowNavMenuTests()
        {
            _ea = _mocker.UseTestEa();
            _rm = new RegionManagerNavigationDecorator(new RegionManager(), _ea);
            _mocker.Use<IRegionManager>(_rm);
            _appState = _mocker.UseImpl<AppState>();
            _mocker.UseImpl<NavigationBreadcrumbsViewModel>();
            _vm = _mocker.CreateInstance<MainWindowViewModel>();
            _vm.IsDataItemEnabled = _vm.IsNetworkItemEnabled = _vm.IsTrainingItemEnabled = _vm.IsPredictionItemEnabled = true;

            _rm.Regions.Add(AppRegions.ContentRegion, new Region());
        }

        private void NavigateContent(string breadcrumb, bool isModal)
        {
            _rm.Regions[AppRegions.ContentRegion].RemoveAll();

            var view = new DependencyObject();
            view.SetValue(BreadcrumbsHelper.BreadcrumbProperty, breadcrumb);
            view.SetValue(BreadcrumbsHelper.IsModalProperty, isModal);
            _rm.Regions[AppRegions.ContentRegion].Add(view);
            _rm.Regions[AppRegions.ContentRegion].Activate(view);

            _ea.GetEvent<ContentRegionViewChanged>().Publish();
        }

        [Fact]
        public void CheckNavItem_event_when_received___publishes_preview_event()
        {
            //act
            _ea.GetEvent<CheckNavMenuItem>().Publish(ModuleIds.Data);

            //assert
            _ea.VerifyTimesCalled<PreviewCheckNavMenuItem>(Times.Once());
        }

        [Fact]
        public void ContentRegion_navigation___sets_breadcrumbs()
        {
            //act
            NavigateContent("b1", false);

            //assert
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("b1");
        }

        [Fact]
        public void NavItem_check___sets_breadcrumbs_saves_and_restores()
        {
            bool netNav=false, dataNav=false;

            //arrange
            _ea.GetEvent<PreviewCheckNavMenuItem>().Subscribe(arg =>
            {
                //navigation for modules
                if (arg.Next == ModuleIds.NeuralNetwork && !netNav)
                {
                    netNav = true;
                    NavigateContent("by", false);
                }

                if (arg.Next == ModuleIds.Data && !dataNav)
                {
                    dataNav = true;
                    NavigateContent("bx", false);
                }
            });

            _vm.CheckedNavItemId = ModuleIds.Data;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("bx");
            

            //act
            _vm.CheckedNavItemId = ModuleIds.NeuralNetwork;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("by");

            //should be restored
            _vm.CheckedNavItemId = ModuleIds.Data;

            //assert
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("bx");

            //should be restored
            _vm.CheckedNavItemId = ModuleIds.NeuralNetwork;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("by");
        }

        [Fact]
        public void Reload_and_setup_navigation_events_are_published_when_active_session_is_changed()
        {
            var reloadTimesCalled = new int[] { 0, 0, 0, 0, 0,0 };
            var startNewNavTimesCalled = new int[] { 0, 0, 0, 0, 0,0 };

            void ResetCounters()
            {
                reloadTimesCalled = new int[] { 0, 0, 0, 0, 0,0 };
                startNewNavTimesCalled = new int[] { 0, 0, 0, 0, 0,0 };
            }


            _ea.GetEvent<ReloadContentForSession>().Subscribe(args => reloadTimesCalled[args.moduleId]++);
            _ea.GetEvent<SetupNewNavigationForSession>().Subscribe(args => startNewNavTimesCalled[args.moduleId]++);

            _vm.CheckedNavItemId = ModuleIds.Data;

            _appState.CreateSession();



            reloadTimesCalled[ModuleIds.Data].Should().Be(0);
            startNewNavTimesCalled[ModuleIds.NeuralNetwork].Should().Be(0);
            startNewNavTimesCalled[ModuleIds.Shell].Should().Be(0);
            startNewNavTimesCalled[ModuleIds.Training].Should().Be(0);

            _appState.ActiveSession = _appState.CreateSession();

            reloadTimesCalled[ModuleIds.Data].Should().Be(1);
            startNewNavTimesCalled[ModuleIds.Data].Should().Be(0);

            startNewNavTimesCalled[ModuleIds.NeuralNetwork].Should().Be(1);
            startNewNavTimesCalled[ModuleIds.Shell].Should().Be(0);
            startNewNavTimesCalled[ModuleIds.Training].Should().Be(1);

            ResetCounters();

            _vm.CheckedNavItemId = ModuleIds.Training;

            _appState.ActiveSession = _appState.CreateSession();


            startNewNavTimesCalled[ModuleIds.Data].Should().Be(1);
            startNewNavTimesCalled[ModuleIds.NeuralNetwork].Should().Be(1);
            startNewNavTimesCalled[ModuleIds.Shell].Should().Be(0);

            startNewNavTimesCalled[ModuleIds.Training].Should().Be(0);
            reloadTimesCalled[ModuleIds.Training].Should().Be(1);
        }
    }
}