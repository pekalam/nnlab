using Prism.Regions;

namespace Shell.Interface
{
    public static class RegionManagerExtensions
    {
        public static void NavigateContentRegion(this IRegionManager rm, string viewName,
            ContentRegionNavigationParameters navParams)
        {
            rm.RequestNavigate(AppRegions.ContentRegion, viewName, navParams);
        }


        public static void NavigateContentRegion(this IRegionManager rm, string viewName, string breadcrumb)
        {
            rm.RequestNavigate(AppRegions.ContentRegion, viewName, new ContentRegionNavigationParameters(breadcrumb));
        }
    }
}