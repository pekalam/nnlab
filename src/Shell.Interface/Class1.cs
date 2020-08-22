using System.Runtime.CompilerServices;
using CommonServiceLocator;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;

namespace Shell.Interface
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

    public class ErrorNotificationArgs
    {
        public string Message { get; set; }
    }

    public class ShowErrorNotification : PubSubEvent<ErrorNotificationArgs> { }

    public class HideErrorNotification : PubSubEvent { }
}
