using System;
using System.Windows.Controls;
using Common.Framework.Extensions;
using Prism.Events;
using Prism.Regions;

namespace Shell.Interface
{
    public interface IActionMenuNavigationService
    {
        void SetLeftMenu<T>() where T : UserControl, new();
        void SetRightMenu<T>() where T : UserControl, new();
    }

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

    public static class EventAggregatorExtenstions
    {
        public static void OnFirstNavigation(this IEventAggregator ea, int moduleNavId, Action action)
        {
            ea.GetEvent<PreviewCheckNavMenuItem>().SubscribeOnceWhen(_ => action(), args => args.Next == moduleNavId);
        }
    }

    public static class AppRegions
    {
        public static string ContentRegion = nameof(ContentRegion);
        public static string FlyoutRegion = nameof(FlyoutRegion);
        public static string TopMenuRightRegion = nameof(TopMenuRightRegion);
        public static string TopMenuLeftRegion = nameof(TopMenuLeftRegion);
        public static string ActionMenuRightRegion = nameof(ActionMenuRightRegion);
        public static string ActionMenuLeftRegion = nameof(ActionMenuLeftRegion);
    }


    class Class1
    {
    }

    public class FlyoutArgs
    {
        public string Title { get; set; }
    }

    public class ShowFlyout : PubSubEvent<FlyoutArgs>
    {

    }

    public class HideFlyout : PubSubEvent { }


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

    public class ContentRegionViewChangedEventArgs
    {
        public string ViewName { get; set; }
        public ContentRegionNavigationParameters NavigationParameters { get; set; }
    }

    public class ContentRegionViewChanged : PubSubEvent<ContentRegionViewChangedEventArgs>
    {
    }

    /// <summary>
    /// Parameter: module nav identifier
    /// </summary>
    public class EnableNavMenuItem : PubSubEvent<int> { }

    /// <summary>
    /// Parameter: module nav identifier
    /// </summary>
    public class DisableNavMenuItem : PubSubEvent<int> { }


    /// <summary>
    /// Parameter: module nav identifier
    /// </summary>
    public class CheckNavMenuItem : PubSubEvent<int> { }

    public class PreviewCheckNavMenuItemArgs
    {
        public PreviewCheckNavMenuItemArgs(int current, int next)
        {
            Current = current;
            Next = next;
        }

        public int Current { get; set; }
        public int Next { get; set; }
    }

    public class PreviewCheckNavMenuItem : PubSubEvent<PreviewCheckNavMenuItemArgs> { }
}
