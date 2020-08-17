using System;
using System.Collections.Generic;
using System.Text;
using Prism.Events;

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
}
