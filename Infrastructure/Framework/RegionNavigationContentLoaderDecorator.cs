using Prism.Regions;

namespace Infrastructure
{
    /// <summary>
    /// Decorator class that's responsible for adding "view" parameter when navigating in region.
    /// </summary>
    internal class RegionNavigationContentLoaderDecorator : IRegionNavigationContentLoader
    {
        private readonly RegionNavigationContentLoader _baseLoader;

        public RegionNavigationContentLoaderDecorator(RegionNavigationContentLoader baseLoader)
        {
            _baseLoader = baseLoader;
        }

        public object LoadContent(IRegion region, NavigationContext navigationContext)
        {
            var view = _baseLoader.LoadContent(region, navigationContext);

            navigationContext.Parameters.Add("view", view);

            return view;
        }
    }
}