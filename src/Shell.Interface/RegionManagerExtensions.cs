using Prism.Regions;

namespace Shell.Interface
{
    public static class RegionManagerExtensions
    {
        public static void NavigateContentRegion(this IRegionManager rm, string viewName)
        {
            rm.RequestNavigate(AppRegions.ContentRegion, viewName);
        }

        public static void NavigateContentRegion(this IRegionManager rm, string viewName, NavigationParameters navigationParameters)
        {
            rm.RequestNavigate(AppRegions.ContentRegion, viewName, navigationParameters);
        }
    }
}