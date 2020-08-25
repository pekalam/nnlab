using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Castle.DynamicProxy;
using FluentAssertions;
using Moq;
using Prism.Events;
using IInvocation = Castle.DynamicProxy.IInvocation;

namespace TestUtils
{
    internal class EventBaseInterceptor : IInterceptor
    {
        private TestEa _ea;
        private Type _eventType;

        public EventBaseInterceptor(TestEa ea, Type eventType)
        {
            _ea = ea;
            _eventType = eventType;
        }

        public void Intercept(IInvocation invocation)
        {
            var n = invocation.Method.Name;

            if (n == "Publish")
                _ea.AddCalled(_eventType, invocation.Arguments.Length > 0 ? invocation.Arguments[0] : null);

            invocation.Proceed();
        }
    }

    public class TestEa : IEventAggregator
    {
        private readonly IEventAggregator _ea;
        private readonly Dictionary<Type, int> _timesCalled = new Dictionary<Type, int>();
        private readonly Dictionary<Type, List<object>> _publishCallArgs = new Dictionary<Type, List<object>>();
        private readonly Dictionary<Type, object> _proxiedEvents = new Dictionary<Type, object>();

        public TestEa(IEventAggregator ea)
        {
            _ea = ea;
        }

        public IEventAggregator ProxiedEa => _ea;
        public IReadOnlyDictionary<Type, int> EventTimesCalled => _timesCalled;
        public IReadOnlyDictionary<Type, List<object>> PublishCallArgs => _publishCallArgs;

        public void AddCalled(Type eventType, object? arg = null)
        {
            if (arg != null)
            {
                _publishCallArgs[eventType].Add(arg);
            }

            _timesCalled[eventType]++;
        }

        public TEventType GetEvent<TEventType>() where TEventType : EventBase, new()
        {
            var ev = _ea.GetEvent<TEventType>();
            if (_proxiedEvents.ContainsKey(typeof(TEventType))) return (TEventType)_proxiedEvents[typeof(TEventType)];

            var proxied = new ProxyGenerator().CreateClassProxyWithTarget<TEventType>(ev,
                new EventBaseInterceptor(this, ev.GetType()));
            _proxiedEvents[typeof(TEventType)] = proxied;
            _publishCallArgs[typeof(TEventType)] = new List<object>();
            _timesCalled[typeof(TEventType)] = 0;
            return proxied;
        }

        public void VerifyTimesCalled<TEventType>(Times times) where TEventType : EventBase, new()
        {
            if (!EventTimesCalled.ContainsKey(typeof(TEventType)))
            {
                if (times.Equals(Times.Never())) return;
                throw new ArgumentException(nameof(TEventType) + " was not called");
            }

            var (from, to) = times;
            EventTimesCalled[typeof(TEventType)].Should().BeGreaterOrEqualTo(from);
            EventTimesCalled[typeof(TEventType)].Should().BeLessOrEqualTo(to);
        }

        public void VerifyTimesCalled<TEventType>(int times) where TEventType : EventBase, new()
        {
            if (!EventTimesCalled.ContainsKey(typeof(TEventType)))
            {
                if (times == 0) return;
                throw new ArgumentException(nameof(TEventType) + " was not called");
            }

            EventTimesCalled[typeof(TEventType)].Should().Be(times);
        }

        public void VerifyTimesCalled(Type evType, int times)
        {
            if (!EventTimesCalled.ContainsKey(evType))
            {
                if (times == 0) return;
                throw new ArgumentException(evType + " was not called");
            }

            EventTimesCalled[evType].Should().Be(times);
        }
    }
}
