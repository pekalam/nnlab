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
using System.Windows;
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
        RegionManager rm = new RegionManager();
        NavigationBreadcrumbsViewModel vm;

        public NavigationBreadcrumbsTests()
        {
            vm = new NavigationBreadcrumbsViewModel(ea, rm);
            rm.Regions.Add(AppRegions.ContentRegion, new Region());
        }

        private void PublishContentRegionViewChanged(string breadcrumb, bool isModal)
        {
            rm.Regions[AppRegions.ContentRegion].RemoveAll();

            var view = new DependencyObject();
            view.SetValue(BreadcrumbsHelper.BreadcrumbProperty, breadcrumb);
            view.SetValue(BreadcrumbsHelper.IsModalProperty, isModal);
            rm.Regions[AppRegions.ContentRegion].Add(view);
            rm.Regions[AppRegions.ContentRegion].Activate(view);

            ea.GetEvent<ContentRegionViewChanged>().Publish();
        }

        [Fact]
        public void NonModal_navigation_parameters_replaces_breadcrumb()
        {
            PublishContentRegionViewChanged("view1b1", false);
            PublishContentRegionViewChanged("view1b2", true);
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1", "view1b2");
            
            PublishContentRegionViewChanged("view2b1", false);
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view2b1");
        }

        [Fact]
        public void Breadcrumbs_are_changed_whan_content_region_changes()
        {

            PublishContentRegionViewChanged("view1b1", true);
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs.Count.Should().Be(1);

            PublishContentRegionViewChanged("view1b1", true);
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs.Count.Should().Be(1);

            PublishContentRegionViewChanged("view2b2", true);
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1", "view2b2");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs[1].Breadcrumb.Should().NotBeEmpty();

            PublishContentRegionViewChanged("view1b1", true);
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1");
            vm.Breadcrumbs[0].Breadcrumb.Should().NotBeEmpty();
            vm.Breadcrumbs.Count.Should().Be(1);
        }

        [Fact]
        public void Save_and_restore_breadcrumbs_test()
        {
            //data module navigation
            PublishContentRegionViewChanged("view1b1", true);
            PublishContentRegionViewChanged("view11b1", true);

            //save
            vm.SaveBreadcrumbsForModule(ModuleIds.Data);

            //assert that breadcrumbs are saved and cleared
            vm.PreviousBreadcrumbs[ModuleIds.Data].Select(b => b.Breadcrumb)
                .Should().BeEquivalentTo("view1b1", "view11b1");
            vm.Breadcrumbs.Count.Should().Be(0);

            //net module navigation
            PublishContentRegionViewChanged("view2b1", true);
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view2b1");

            //save
            vm.SaveBreadcrumbsForModule(ModuleIds.NeuralNetwork);

            //assert that breadcrumbs are saved and cleared
            vm.PreviousBreadcrumbs[ModuleIds.NeuralNetwork].Select(b => b.Breadcrumb).Should().BeEquivalentTo("view2b1");
            vm.Breadcrumbs.Count.Should().Be(0);

            //restore for data module
            vm.TryRestoreBreadrumbsForModule(ModuleIds.Data);
            //assert
            vm.Breadcrumbs.Select(b => b.Breadcrumb).Should().BeEquivalentTo("view1b1", "view11b1");
        }
    }
}