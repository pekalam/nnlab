using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Prism.Regions;
using Shell.Interface;

namespace Shell.Application.PrismDecorators
{
    internal class RegionCollectionDecorator : IRegionCollection
    {
        private readonly IRegionCollection _regionCollection;
        private readonly Action<Uri, ContentRegionNavigationParameters> _navigationAction;

        public RegionCollectionDecorator(IRegionCollection regionCollection,
            Action<Uri, ContentRegionNavigationParameters> navigationAction)
        {
            _regionCollection = regionCollection;
            _navigationAction = navigationAction;
        }


        protected virtual IRegion GetRegionDecorator(IRegion region)
        {
            return new ContentRegionDecorator(region, _navigationAction);
        }


        public IEnumerator<IRegion> GetEnumerator()
        {
            return _regionCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_regionCollection).GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add => _regionCollection.CollectionChanged += value;
            remove => _regionCollection.CollectionChanged -= value;
        }

        public void Add(IRegion region)
        {
            _regionCollection.Add(region);
        }

        public bool Remove(string regionName)
        {
            return _regionCollection.Remove(regionName);
        }

        public bool ContainsRegionWithName(string regionName)
        {
            return _regionCollection.ContainsRegionWithName(regionName);
        }

        public void Add(string regionName, IRegion region)
        {
            _regionCollection.Add(regionName, region);
        }

        public IRegion this[string regionName]
        {
            get
            {
                if (regionName == AppRegions.ContentRegion)
                {
                    var region = _regionCollection[regionName];
                    return GetRegionDecorator(region);
                }

                return _regionCollection[regionName];
            }
        }
    }
}