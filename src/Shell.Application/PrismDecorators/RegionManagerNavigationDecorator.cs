using System;
using System.Diagnostics;
using Prism.Events;
using Prism.Regions;
using Shell.Interface;
using Unity;

namespace Shell.Application.PrismDecorators
{
    public class RegionManagerNavigationDecorator : IRegionManager
    {
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _ea;

        [InjectionConstructor]
        public RegionManagerNavigationDecorator(RegionManager regionManager, IEventAggregator ea)
        {
            _regionManager = regionManager;
            _ea = ea;
        }

        ///test ctor
        internal RegionManagerNavigationDecorator(IRegionManager regionManager, IEventAggregator ea)
        {
            _regionManager = regionManager;
            _ea = ea;
        }

        protected void SendNavigationEvent()
        {
            _ea.GetEvent<ContentRegionViewChanged>().Publish();
        }

        protected void TrySendNavigationEventForRegion(string regionName)
        {
            if (regionName == AppRegions.ContentRegion)
            {
                SendNavigationEvent();
            }
        }


        protected virtual IRegionCollection GetDecoratedRegionCollection(IRegionCollection regions)
        {
            return new RegionCollectionDecorator(_regionManager.Regions, SendNavigationEvent);
        }

        public IRegionManager CreateRegionManager()
        {
            return _regionManager.CreateRegionManager();
        }

        public IRegionManager AddToRegion(string regionName, object view)
        {
            return _regionManager.AddToRegion(regionName, view);
        }

        public IRegionManager RegisterViewWithRegion(string regionName, Type viewType)
        {
            return _regionManager.RegisterViewWithRegion(regionName, viewType);
        }

        public IRegionManager RegisterViewWithRegion(string regionName, Func<object> getContentDelegate)
        {
            return _regionManager.RegisterViewWithRegion(regionName, getContentDelegate);
        }

        public void RequestNavigate(string regionName, Uri source, Action<NavigationResult> navigationCallback)
        {
            _regionManager.RequestNavigate(regionName, source, navigationCallback);
            TrySendNavigationEventForRegion(regionName);
        }

        public void RequestNavigate(string regionName, Uri source)
        {
            _regionManager.RequestNavigate(regionName, source);
            TrySendNavigationEventForRegion(regionName);
        }

        public void RequestNavigate(string regionName, string source, Action<NavigationResult> navigationCallback)
        {
            _regionManager.RequestNavigate(regionName, source, navigationCallback);
            TrySendNavigationEventForRegion(regionName);
        }

        public void RequestNavigate(string regionName, string source)
        {
            _regionManager.RequestNavigate(regionName, source);
            TrySendNavigationEventForRegion(regionName);
        }

        public void RequestNavigate(string regionName, Uri target, Action<NavigationResult> navigationCallback,
            NavigationParameters navigationParameters)
        {
            

            _regionManager.RequestNavigate(regionName, target, navigationCallback, navigationParameters);
            TrySendNavigationEventForRegion(regionName);
        }

        public void RequestNavigate(string regionName, string target, Action<NavigationResult> navigationCallback,
            NavigationParameters navigationParameters)
        {
            

            _regionManager.RequestNavigate(regionName, target, navigationCallback, navigationParameters);
            TrySendNavigationEventForRegion(regionName);
        }

        public void RequestNavigate(string regionName, Uri target, NavigationParameters navigationParameters)
        {
            

            _regionManager.RequestNavigate(regionName, target, navigationParameters);
            TrySendNavigationEventForRegion(regionName);
        }

        public void RequestNavigate(string regionName, string target, NavigationParameters navigationParameters)
        {
            

            _regionManager.RequestNavigate(regionName, target, navigationParameters);
            TrySendNavigationEventForRegion(regionName);
        }

        public IRegionCollection Regions => GetDecoratedRegionCollection(_regionManager.Regions);
    }
}