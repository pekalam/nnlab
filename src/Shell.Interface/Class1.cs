﻿using Common.Domain;
using Prism.Commands;
using Prism.Events;

namespace Shell.Interface
{
    public class ShowGlobalLoading : PubSubEvent<string?>{}
    public class HideGlobalLoading : PubSubEvent{}
    public class ContentRestoredForModule : PubSubEvent<int>
    {

    }

    public class EnableModalNavigation : PubSubEvent<DelegateCommand>
    {

    }

    public class DisableModalNavigation : PubSubEvent
    {

    }

    public class SetupNewNavigationForSession : PubSubEvent<(int moduleId, Session prev, Session next)> { }

    public class ReloadContentForSession : PubSubEvent<(int moduleId, Session prev, Session next)> { }

    public class ProgressAreaArgs
    {
        public string Message { get; set; } = null!;
        public string Tooltip { get; set; } = null!;
    }

    public class ShowProgressArea : PubSubEvent<ProgressAreaArgs>
    {
    }

    public class HideProgressAreaArgs
    {
        public bool Immediately { get; set; }
        public string? HideMessage { get; set; }
    }

    public class HideProgressArea : PubSubEvent<HideProgressAreaArgs?> { }

    public class FlyoutArgs
    {
        public string? Title { get; set; }
    }

    public class ShowFlyout : PubSubEvent<FlyoutArgs>
    {

    }

    public class HideFlyout : PubSubEvent { }

    public class ContentRegionViewChanged : PubSubEvent
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
        public string? Message { get; set; }
    }

    public class ShowErrorNotification : PubSubEvent<ErrorNotificationArgs> { }

    public class HideErrorNotification : PubSubEvent { }
}
