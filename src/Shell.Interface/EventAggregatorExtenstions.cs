using System;
using Common.Framework.Extensions;
using Prism.Events;

namespace Shell.Interface
{
    public static class EventAggregatorExtenstions
    {
        public static void OnFirstNavigation(this IEventAggregator ea, int moduleNavId, Action action)
        {
            ea.GetEvent<PreviewCheckNavMenuItem>().SubscribeOnceWhen(_ => action(), args => args.Next == moduleNavId);
        }
    }
}