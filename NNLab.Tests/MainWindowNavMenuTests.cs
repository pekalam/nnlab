using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data;
using FluentAssertions;
using Infrastructure.Extensions;
using Infrastructure.Messaging;
using Infrastructure.PrismDecorators;
using Moq;
using Moq.AutoMock;
using NeuralNetwork;
using NNLab.ViewModels;
using Prism.Regions;
using TestUtils;
using Xunit;

namespace NNLab.Tests
{
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
            _ea.GetEvent<CheckNavMenuItem>().Publish(DataModule.NavIdentifier);

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
                if (arg.Next == NeuralNetworkModule.NavIdentifier && !netNav)
                {
                    netNav = true;
                    _rm.Object.NavigateContentRegion("y", "by");
                    _rm.Object.NavigateContentRegion("z", "bz");
                }

                if (arg.Next == DataModule.NavIdentifier && !dataNav)
                {
                    dataNav = true;
                    _rm.Object.NavigateContentRegion("x", "bx");
                }
            });

            _vm.CheckedNavItemId = DataModule.NavIdentifier;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("bx");
            

            //act
            _vm.CheckedNavItemId = NeuralNetworkModule.NavIdentifier;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("by", "bz");

            //should be restored
            _vm.CheckedNavItemId = DataModule.NavIdentifier;

            //assert
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("bx");

            //should be restored
            _vm.CheckedNavItemId = NeuralNetworkModule.NavIdentifier;
            _vm.NavigationBreadcrumbsVm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("by", "bz");
        }
    }
}