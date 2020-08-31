using System;
using Prism.Events;

namespace Common.Framework.Extensions
{
    public static class PubSubEventExtensions
    {
        public static void SubscribeOnceWhen<TPayload>(this PubSubEvent<TPayload> ev, Action<TPayload> action, Func<TPayload, bool> predicate) where TPayload : class
        {
            SubscriptionToken? _token = null;
            SubscriptionToken token = ev.Subscribe(payload => {
                if (predicate(payload))
                {
                    action(payload);
                    _token!.Dispose();
                }

            }, keepSubscriberReferenceAlive: true);
            _token = token;
        }
    }
}
