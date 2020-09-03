using System;
using System.ComponentModel;
using System.Diagnostics;
using Prism.Regions;
using Shell.Interface;

namespace Shell.Application.PrismDecorators
{
    internal class ContentRegionDecorator : IRegion
    {
        private readonly IRegion _region;
        private readonly Action _navigationAction;

        public ContentRegionDecorator(IRegion region, Action navigationAction)
        {
            _region = region;
            _navigationAction = navigationAction;
        }


        public void RequestNavigate(Uri target, Action<NavigationResult> navigationCallback)
        {
            _region.RequestNavigate(target, navigationCallback);
            _navigationAction.Invoke();
        }

        public void RequestNavigate(Uri target, Action<NavigationResult> navigationCallback,
            NavigationParameters navigationParameters)
        {
            _region.RequestNavigate(target, navigationCallback, navigationParameters);
            _navigationAction.Invoke();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => _region.PropertyChanged += value;
            remove => _region.PropertyChanged -= value;
        }

        public IRegionManager Add(object view)
        {
            return _region.Add(view);
        }

        public IRegionManager Add(object view, string viewName)
        {
            return _region.Add(view, viewName);
        }

        public IRegionManager Add(object view, string viewName, bool createRegionManagerScope)
        {
            return _region.Add(view, viewName, createRegionManagerScope);
        }

        public void Remove(object view)
        {
            _region.Remove(view);
        }

        public void RemoveAll()
        {
            _region.RemoveAll();
        }

        public void Activate(object view)
        {
            _region.Activate(view);
        }

        public void Deactivate(object view)
        {
            _region.Deactivate(view);
        }

        public object GetView(string viewName)
        {
            return _region.GetView(viewName);
        }

        public IViewsCollection Views => _region.Views;

        public IViewsCollection ActiveViews => _region.ActiveViews;

        public object Context
        {
            get => _region.Context;
            set => _region.Context = value;
        }

        public string Name
        {
            get => _region.Name;
            set => _region.Name = value;
        }

        public Comparison<object> SortComparison
        {
            get => _region.SortComparison;
            set => _region.SortComparison = value;
        }

        public IRegionManager RegionManager
        {
            get => _region.RegionManager;
            set => _region.RegionManager = value;
        }

        public IRegionBehaviorCollection Behaviors => _region.Behaviors;

        public IRegionNavigationService NavigationService
        {
            get => _region.NavigationService;
            set => _region.NavigationService = value;
        }
    }
}