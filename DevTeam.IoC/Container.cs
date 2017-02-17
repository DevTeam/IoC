namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    public class Container : IContainer, IObservable<IRegistrationEvent>
    {
        private readonly List<IDisposable> _resources = new List<IDisposable>();
        private readonly Dictionary<IEqualityComparer<ICompositeKey>, Dictionary<ICompositeKey, RegistrationItem>> _registrations = new Dictionary<IEqualityComparer<ICompositeKey>, Dictionary<ICompositeKey, RegistrationItem>>();
        private readonly Subject<IRegistrationEvent> _registrationSubject = new Subject<IRegistrationEvent>();
        private readonly IKeyFactory _keyFactory;
        private ICache<ICompositeKey, IResolverContext> _cache;

        public Container([CanBeNull] object tag = null)
        {
            Tag = tag;
            _resources.Add(new CompositeDisposable(RootContainerConfiguration.Shared.Apply(this)));
            _resources.Add(new CompositeDisposable(ContainerConfiguration.Shared.Apply(this)));
            this.TryResolve(out _keyFactory);
            this.TryResolve(out _cache);
            var cacheTracker = new CacheTracker(this);
            _resources.Add(Subscribe(cacheTracker));
        }

        internal Container([NotNull] IContainer parentContainer, [CanBeNull] object tag = null, [CanBeNull] IResolverContext resolverContext = null)
        {
            Tag = tag;
            Parent = parentContainer;
            _resources.Add(new CompositeDisposable(ContainerConfiguration.Shared.Apply(this)));
            this.TryResolve(out _keyFactory);
            this.TryResolve(out _cache);
            var cacheTracker = new CacheTracker(this);
            _resources.Add(Subscribe(cacheTracker));
            var parent = Parent;
            do
            {
                var observableParent = parent as IObservable<IRegistrationEvent>;
                if (observableParent != null)
                {
                    _resources.Add(observableParent.Subscribe(cacheTracker));
                }

                parent = parent.Parent;
            }
            while (parent != null);
        }

        public object Tag { get; }

        public IEnumerable<ICompositeKey> Registrations => _registrations.SelectMany(i => i.Value).Select(i => i.Key);

        public IContainer Parent { get; }

        public IKeyFactory KeyFactory => _keyFactory ?? RootContainerConfiguration.KeyFactory;

        private object LockObject => _registrations;

        public IRegistryContext CreateRegistryContext(IEnumerable<ICompositeKey> keys, IResolverFactory factory, IEnumerable<IExtension> extensions)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
            var keysArr = keys.ToArray();
            if (keysArr.Length == 0) throw new ArgumentException("Value cannot be an empty enumerable.", nameof(keys));

            lock (LockObject)
            {
                return new RegistryContext(this, keysArr, factory, extensions);
            }
        }

        public bool TryRegister(IRegistryContext context, out IDisposable registration)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var resources = new List<IDisposable>();
            var registrationItem = new RegistrationItem(context, new LifetimesFactory(context.Extensions.OfType<ILifetime>()), resources);

            lock (LockObject)
            {
                var scope = registrationItem.Scope;
                if (scope!= null && !scope.AllowsRegistration(context) && Parent != null)
                {
                    return Parent.TryRegister(context, out registration);
                }

                var comparer = registrationItem.KeyComparer != null ? (IEqualityComparer<ICompositeKey>)registrationItem.KeyComparer : EqualityComparer<IKey>.Default;
                Dictionary<ICompositeKey, RegistrationItem> registrations;
                if (!_registrations.TryGetValue(comparer, out registrations))
                {
                    registrations = new Dictionary<ICompositeKey, RegistrationItem>(comparer);
                    _registrations.Add(comparer, registrations);
                }

                try
                {
                    foreach (var key in context.Keys)
                    {
                        var currentRegistration = registrationItem;
                        _registrationSubject.OnNext(new RegistrationEvent(EventStage.Before, EventAction.Add, key, currentRegistration.RegistryContext));
                        registrations.Add(key, currentRegistration);
                        resources.Add(
                            new Disposable(() =>
                            {
                                _registrationSubject.OnNext(new RegistrationEvent(EventStage.Before, EventAction.Remove, key, currentRegistration.RegistryContext));
                                registrations.Remove(key);
                                _registrationSubject.OnNext(new RegistrationEvent(EventStage.After, EventAction.Remove, key, currentRegistration.RegistryContext));
                            },
                            this));

                        _registrationSubject.OnNext(new RegistrationEvent(EventStage.After, EventAction.Add, key, currentRegistration.RegistryContext));
                    }
                }
                catch
                {
                    registrationItem.Dispose();
                    registration = default(IDisposable);
                    return false;
                }

                registration = registrationItem;
                return true;
            }
        }

        public bool TryCreateResolverContext(ICompositeKey key, out IResolverContext resolverContext, IStateProvider stateProvider = null, IContainer container = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var isRootResolver = false;
            if (container == null)
            {
                container = this;
                isRootResolver = true;
            }

            lock (LockObject)
            {
                if (_cache != null && _cache.TryGet(key, out resolverContext))
                {
                    return true;
                }

                foreach (var registrations in _registrations)
                {
                    RegistrationItem registrationItem;
                    if (!registrations.Value.TryGetValue(key, out registrationItem))
                    {
                        continue;
                    }

                    resolverContext = new ResolverContext(
                        container,
                        registrationItem.RegistryContext,
                        registrationItem.InstanceFactory,
                        key,
                        registrationItem.Key,
                        stateProvider);

                    var scope = registrationItem.Scope;
                    if (scope == null || scope.AllowsResolving(resolverContext))
                    {
                        if (isRootResolver)
                        {
                            _cache?.Set(key, resolverContext);
                        }

                        return true;
                    }
                }

                if (Parent != null && Parent.TryCreateResolverContext(key, out resolverContext, stateProvider, container))
                {
                    if (isRootResolver)
                    {
                        _cache?.Set(key, resolverContext);
                    }

                    return true;
                }

                resolverContext = default(IResolverContext);
                return false;
            }
        }

        public object Resolve(IResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Container == this)
            {
                return context.InstanceFactory.Create(context);
            }

            if (Parent != null)
            {
                return Parent.Resolve(context);
            }

            throw new InvalidOperationException("Invalid container context.");
        }

        public void Dispose()
        {
            foreach (var registration in _registrations.ToList().SelectMany(i => i.Value).ToList())
            {
                var registryContext = registration.Value.RegistryContext;
                _registrationSubject.OnNext(new RegistrationEvent(EventStage.Before, EventAction.Remove, registration.Key, registryContext));
                registration.Value.Dispose();
                _registrationSubject.OnNext(new RegistrationEvent(EventStage.After, EventAction.Remove, registration.Key, registryContext));
            }

            _registrations.Clear();
            _registrationSubject.OnCompleted();

            foreach (var resource in _resources)
            {
                resource.Dispose();
            }
        }

        public IDisposable Subscribe([NotNull] IObserver<IRegistrationEvent> observer)
        {
            return _registrationSubject.Subscribe(observer);
        }

        public override string ToString()
        {
            return $"{nameof(Container)} [Tag: {Tag ?? "null"}]{Environment.NewLine}{string.Join(Environment.NewLine, Registrations)}";
        }

        private class CacheTracker : IObserver<IRegistrationEvent>
        {
            private readonly Container _container;

            public CacheTracker(Container container)
            {
                _container = container;
            }

            public void OnNext(IRegistrationEvent value)
            {
                if (value.Stage == EventStage.After)
                {
                    _container._cache?.TryRemove(value.Key);
                }
            }

            public void OnError(Exception error)
            {
            }

            public void OnCompleted()
            {
            }
        }
    }
}
