using Common.Domain;
using FluentAssertions;
using Moq;
using Prism.Events;
using Prism.Regions;
using Shell.Application.ViewModels;
using Shell.Interface;
using System;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace Shell.Application.Tests
{
    class TestRegion : IRegion
    {
        public Action<Uri, Action<NavigationResult>, NavigationParameters> NavCalled;

        public void RequestNavigate(Uri target, Action<NavigationResult> navigationCallback)
        {
            throw new NotImplementedException();
        }

        public void RequestNavigate(Uri target, Action<NavigationResult> navigationCallback, NavigationParameters navigationParameters)
        {
            NavCalled(target, navigationCallback, navigationParameters);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public IRegionManager Add(object view)
        {
            throw new NotImplementedException();
        }

        public IRegionManager Add(object view, string viewName)
        {
            throw new NotImplementedException();
        }

        public IRegionManager Add(object view, string viewName, bool createRegionManagerScope)
        {
            throw new NotImplementedException();
        }

        public void Remove(object view)
        {
            throw new NotImplementedException();
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }

        public void Activate(object view)
        {
            throw new NotImplementedException();
        }

        public void Deactivate(object view)
        {
            throw new NotImplementedException();
        }

        public object GetView(string viewName)
        {
            throw new NotImplementedException();
        }

        public IViewsCollection Views { get; }
        public IViewsCollection ActiveViews { get; }
        public object Context { get; set; }
        public string Name { get; set; }
        public Comparison<object> SortComparison { get; set; }
        public IRegionManager RegionManager { get; set; }
        public IRegionBehaviorCollection Behaviors { get; }
        public IRegionNavigationService NavigationService { get; set; }
    }

    public class NavigationBreadcrumbsTests
    {
        EventAggregator ea = new EventAggregator();
        Mock<IRegionManager> rm = new Mock<IRegionManager>();

        private void PublishContentRegionViewChanged(string viewName, string breadcrumb)
        {
            ea.GetEvent<ContentRegionViewChanged>().Publish(new ContentRegionViewChangedEventArgs()
            {
                ViewName = viewName,
                NavigationParameters = new ContentRegionNavigationParameters(breadcrumb)
            });
        }

        [Fact]
        public void Breadcrumbs_are_changed_whan_content_region_changes()
        {
            var vm = new NavigationBreadcrumbsViewModel(ea, rm.Object);

            PublishContentRegionViewChanged("view1", "view1b1");
            vm.Breadcrumbs.Select(b => b.ViewName).Should().BeEquivalentTo("view1");
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs[0].NavParams.Should().NotBeNull();
            vm.Breadcrumbs.Count.Should().Be(1);

            PublishContentRegionViewChanged("view1", "view1b1");
            vm.Breadcrumbs.Select(b => b.ViewName).Should().BeEquivalentTo("view1");
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs[0].NavParams.Should().NotBeNull();
            vm.Breadcrumbs.Count.Should().Be(1);

            PublishContentRegionViewChanged("view2", "view2b2");
            vm.Breadcrumbs.Select(b => b.ViewName).Should().BeEquivalentTo("view1", "view2");
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1", "view2b2");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs[1].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs[1].NavParams.Should().NotBeNull();

            PublishContentRegionViewChanged("view1", "view1b1");
            vm.Breadcrumbs.Select(b => b.ViewName).Should().BeEquivalentTo("view1");
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs[0].NavParams.Should().NotBeNull();
            vm.Breadcrumbs.Count.Should().Be(1);
        }

        [Fact]
        public void Save_and_restore_breadcrumbs_test()
        {
            //arrange
            var vm = new NavigationBreadcrumbsViewModel(ea, rm.Object);

            //data module navigation
            PublishContentRegionViewChanged("view1", "view1b1");
            PublishContentRegionViewChanged("view11", "view11b1");

            //save
            vm.SaveBreadcrumbsForModule(ModuleIds.Data);

            //assert that breadcrumbs are saved and cleared
            vm.PreviousBreadcrumbs[ModuleIds.Data].Select(b => b.ViewName).Should().BeEquivalentTo("view1", "view11");
            vm.Breadcrumbs.Count.Should().Be(0);

            //net module navigation
            PublishContentRegionViewChanged("view2", "view2b1");
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view2b1");

            //save
            vm.SaveBreadcrumbsForModule(ModuleIds.NeuralNetwork);

            //assert that breadcrumbs are saved and cleared
            vm.PreviousBreadcrumbs[ModuleIds.NeuralNetwork].Select(b => b.ViewName).Should().BeEquivalentTo("view2");
            vm.Breadcrumbs.Count.Should().Be(0);

            //restore for data module
            vm.TryRestoreBreadrumbsForModule(ModuleIds.Data);
            //assert
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1", "view11b1");
        }

        [Fact]
        public void NavigateCmd_calls_request_navigate_for_content_region()
        {
            //arrange
            var mockRegion = new TestRegion();
            rm.Setup(f => f.Regions[AppRegions.ContentRegion]).Returns(mockRegion);
            var vm = new NavigationBreadcrumbsViewModel(ea, rm.Object);

            mockRegion.NavCalled = (uri, callback, navParam) =>
            {
                //assert
                uri.ToString().Should().Contain("view1");
                navParam.Should().BeEquivalentTo(vm.Breadcrumbs[^2]);
            };

            PublishContentRegionViewChanged("view1", "view1b1");
            PublishContentRegionViewChanged("view2", "view2b2");

            //act
            vm.NavigateCommand.Execute(vm.Breadcrumbs[^2]);
        }
    }
}