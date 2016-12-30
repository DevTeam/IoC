﻿namespace DevTeam.IoC.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal class EventBroker : IEventBroker
    {
        private readonly ILog _log;
        private readonly Dictionary<Type, object> _sources = new Dictionary<Type, object>();
        private readonly Dictionary<Type, object> _listeners = new Dictionary<Type, object>();
        private readonly Dictionary<Type, List<IDisposable>> _subscriptions = new Dictionary<Type, List<IDisposable>>();

        public EventBroker(
            [State(0, typeof(string), Value = nameof(EventBroker))] [Contract] ILog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            log.Method("Ctor()");
            _log = log;
        }

        public void Dispose()
        {
            _log.Method("Dispose()");
            _sources.Clear();
            _listeners.Clear();
            foreach (var subscription in _subscriptions)
            {
                foreach (var disposable in subscription.Value)
                {
                    disposable.Dispose();
                }
            }

            _subscriptions.Clear();
        }

        public IDisposable RegisterSource<TEvent>(IEventSource<TEvent> source)
        {
            _log.Method($"RegisterSource({source})");
            var eventType = typeof(TEvent);
            _sources.Add(eventType, source);
            RefreshSubscriptions(eventType);
            return new Subscription(() => RemoveSource(eventType));
        }

        public IDisposable RegisterListener<TEvent>(IEventListener<TEvent> listener)
        {
            _log.Method($"RegisterListener({listener})");
            var eventType = typeof(TEvent);
            _listeners.Add(eventType, listener);
            RefreshSubscriptions(eventType);
            return new Subscription(() => RemoveListener(eventType));
        }

        private void RemoveSource(Type eventType)
        {
            _log.Method($"RemoveSource({eventType})");
            RefreshSubscriptions(eventType);
        }

        private void RemoveListener(Type eventType)
        {
            _log.Method($"RemoveListener({eventType})");
            RefreshSubscriptions(eventType);
        }

        private void RefreshSubscriptions(Type eventType)
        {
            _log.Method($"RefreshSubscriptions({eventType})");
            List<IDisposable> subscriptions;
            if (_subscriptions.TryGetValue(eventType, out subscriptions))
            {
                foreach (var subscription in subscriptions)
                {
                    subscription.Dispose();
                }

                subscriptions.Clear();
            }
            else
            {
                subscriptions = new List<IDisposable>();
                _subscriptions.Add(eventType, subscriptions);
            }

            foreach (var source in _sources)
            {
                var subscribeMethod = source.Value.GetType().GetTypeInfo().GetDeclaredMethod("Subscribe");
                foreach (var listener in _listeners)
                {
                    var subscription = (IDisposable) subscribeMethod.Invoke(source.Value, new[] {listener.Value});
                    subscriptions.Add(subscription);
                }
            }
        }

        public override string ToString()
        {
            return $"{nameof(EventBroker)} [Sources Count: {_sources.Count}, Listeners Count: {_listeners.Count}, Subscriptions Count: {_subscriptions.Count}]";
        }
    }
}