using Prism.Regions;

namespace Shell.Interface
{
    public class ContentRegionNavigationParameters : NavigationParameters
    {
        internal const string BreadcrumbKeyName = "Breadcrumb";

        public ContentRegionNavigationParameters(string breadcrumb, bool showBreadcrumbs = true)
        {
            if (string.IsNullOrWhiteSpace(breadcrumb))
            {
                //TODO
                //throw new ArgumentException("breadcrumbName cannot be null or white space");
            }

            ShowBreadcrumbs = showBreadcrumbs;
            Add(BreadcrumbKeyName, breadcrumb);
        }

        public bool ShowBreadcrumbs { get; }
        public string Breadcrumb => GetValue<string>(BreadcrumbKeyName);
    }
}