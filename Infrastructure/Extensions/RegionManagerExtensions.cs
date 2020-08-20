using Infrastructure.Messaging;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Text;
using Accessibility;
using Prism.Events;

namespace Infrastructure.Extensions
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


    public static class PubSubEventExtensions
    {
        public static void SubscribeOnceWhen<TPayload>(this PubSubEvent<TPayload> ev, Action<TPayload> action, Func<TPayload, bool> predicate) where TPayload : class
        {
            SubscriptionToken _token = null;
            SubscriptionToken token = ev.Subscribe(payload => {
                if (predicate(payload))
                {
                    action(payload);
                    _token.Dispose();
                }

            }, keepSubscriberReferenceAlive: true);
            _token = token;
        }
    }

    public static class EventAggregatorExtenstions
    {
        public static void OnFirstNavigation(this IEventAggregator ea, int moduleNavId, Action action)
        {
            ea.GetEvent<PreviewCheckNavMenuItem>().SubscribeOnceWhen(_ => action(), args => args.Next == moduleNavId);
        }
    }
}
