using System;
using System.Collections.Generic;
using System.Text;
using Prism.Events;
using Prism.Regions;

namespace Infrastructure.Messaging
{
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
