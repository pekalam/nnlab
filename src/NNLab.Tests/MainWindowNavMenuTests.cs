using System.Collections.Generic;
using System.Linq;
using Common.Domain;
using FluentAssertions;
using Moq;
using Moq.AutoMock;
using Prism.Regions;
using Shell.Application.PrismDecorators;
using Shell.Application.ViewModels;
using Shell.Interface;
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
        private TestEa _ea;
        private Mock<RegionManagerNavigationDecorator> _rm;
        private MainWindowViewModel _vm;

        public MainWindowNavMenuTests()
        {
            (_rm, _, _ea) = _mocker.UseDecoratedTestRm();
            _mocker.UseImpl<NavigationBreadcrumbsViewModel>();

            _vm = _mocker.CreateInstance<MainWindowViewModel>();
            _vm.IsDataItemEnabled = _vm.IsNetworkItemEnabled = _vm.IsTrainingItemEnabled = _vm.IsPredictionItemEnabled = true;
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
            _rm.Object.NavigateContentRegion("x", "b1");

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
                    _rm.Object.NavigateContentRegion("y", "by");
                    _rm.Object.NavigateContentRegion("z", "bz");
                }

                if (arg.Next == ModuleIds.Data && !dataNav)
                {
                    dataNav = true;
                    _rm.Object.NavigateContentRegion("x", "bx");
                }
            });

            _vm.CheckedNavItemId = ModuleIds.Data;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("bx");
            

            //act
            _vm.CheckedNavItemId = ModuleIds.NeuralNetwork;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("by", "bz");

            //should be restored
            _vm.CheckedNavItemId = ModuleIds.Data;

            //assert
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("bx");

            //should be restored
            _vm.CheckedNavItemId = ModuleIds.NeuralNetwork;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("by", "bz");
        }
    }
}