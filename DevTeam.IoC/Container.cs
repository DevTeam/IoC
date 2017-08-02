namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    public class Container :
        IContainer,
        IObservable<IRegistrationEvent>,
        IProvider<ICache<Type, IInstanceFactory>>,
        IProvider<IFluent>
    {
        private readonly List<IDisposable> _resources = new List<IDisposable>();
        private readonly Dictionary<IEqualityComparer<IKey>, Dictionary<IKey, RegistrationItem>> _registrations = new Dictionary<IEqualityComparer<IKey>, Dictionary<IKey, RegistrationItem>>();
        private readonly Subject<IRegistrationEvent> _registrationSubject = new Subject<IRegistrationEvent>();
        // ReSharper disable once MemberInitializerValueIgnored
        private readonly IFluent _fluent;
        private readonly IKeyFactory _keyFactory;
        private readonly Cache<IKey, IResolverContext> _resolverContextCache = new Cache<IKey, IResolverContext>();
        private readonly Cache<Type, IInstanceFactory> _resolverFactoryCache = new Cache<Type, IInstanceFactory>();

        public Container([CanBeNull] object tag = null)
        {
            Tag = tag;
            _fluent = Fluent.Shared;
            _resources.Add(new CompositeDisposable(RootContainerConfiguration.Shared.Apply(this)));
            _resources.Add(new CompositeDisposable(ContainerConfiguration.Shared.Apply(this)));
            if (!this.TryResolve(out _keyFactory))
            {
                throw new ContainerException($"Can not resolve IKeyFactory.\nDetails:\n{this}");
            }

            var cacheTracker = new CacheTracker(this);
            _resources.Add(((IObservable<IRegistrationEvent>)this).Subscribe(cacheTracker));
        }

        internal Container([NotNull] IContainer parentContainer, [CanBeNull] object tag = null, [CanBeNull] IResolverContext resolverContext = null)
        {
            Tag = tag;
            _fluent = parentContainer.Resolve().Instance<IFluent>();
            Parent = parentContainer;
            _resources.Add(new CompositeDisposable(ContainerConfiguration.Shared.Apply(this)));
            if (!this.TryResolve(out _fluent))
            {
                throw new ContainerException($"Can not resolve IFluent.\nDetails:\n{this}");
            }

            if (!this.TryResolve(out _keyFactory))
            {
                throw new ContainerException($"Can not resolve IKeyFactory.\nDetails:\n{this}");
            }

            var cacheTracker = new CacheTracker(this);
            _resources.Add(((IObservable<IRegistrationEvent>)this).Subscribe(cacheTracker));
            var parent = Parent;
            do
            {
                if (parent is IObservable<IRegistrationEvent> observableParent)
                {
                    _resources.Add(observableParent.Subscribe(cacheTracker));
                }

                parent = parent.Parent;
            }
            while (parent != null);
        }

        public object Tag { get; }

        public IEnumerable<IKey> Registrations => _registrations.SelectMany(i => i.Value).Select(i => i.Key);

        public IContainer Parent { get; }

        public IKeyFactory KeyFactory => _keyFactory ?? RootContainerConfiguration.KeyFactory;

        private object LockObject => _registrations;

        public IRegistryContext CreateRegistryContext(IEnumerable<IKey> keys, IInstanceFactory factory, params IExtension[] extensions)
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
            var registrationItem = new RegistrationItem(context, resources);

            lock (LockObject)
            {
                var scope = registrationItem.Scope;
                if (scope!= null && !scope.AllowRegistration(context, this))
                {
                    var parent = Parent;
                    if (parent == null)
                    {
                        registrationItem.Dispose();
                        registration = default(IDisposable);
                        return false;
                    }

                    return parent.TryRegister(context, out registration);
                }

                var comparer = registrationItem.KeyComparer != null ? (IEqualityComparer<IKey>)registrationItem.KeyComparer : EqualityComparer<IKey>.Default;
                if (!_registrations.TryGetValue(comparer, out Dictionary<IKey, RegistrationItem> registrations))
                {
                    registrations = new Dictionary<IKey, RegistrationItem>(comparer);
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

        public bool TryCreateResolverContext(IKey key, out IResolverContext resolverContext, IContainer container = null)
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
                if (_resolverContextCache.TryGet(key, out resolverContext))
                {
                    resolverContext = new ResolverContext(
                        container,
                        resolverContext.RegistryContext,
                        resolverContext.InstanceFactory,
                        key);
                    return true;
                }

                foreach (var registrations in _registrations)
                {
                    if (!registrations.Value.TryGetValue(key, out RegistrationItem registrationItem))
                    {
                        continue;
                    }

                    resolverContext = new ResolverContext(
                        container,
                        registrationItem.RegistryContext,
                        registrationItem.InstanceFactory,
                        key);

                    var scope = registrationItem.Scope;
                    if (scope == null || scope.AllowResolving(resolverContext))
                    {
                        if (isRootResolver)
                        {
                            _resolverContextCache.Set(key, resolverContext);
                        }

                        return true;
                    }
                }

                if (Parent != null && Parent.TryCreateResolverContext(key, out resolverContext, container))
                {
                    if (isRootResolver)
                    {
                        _resolverContextCache.Set(key, resolverContext);
                    }

                    return true;
                }

                resolverContext = default(IResolverContext);
                return false;
            }
        }

        public object Resolve(IResolverContext context, IStateProvider stateProvider = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Container == this)
            {
                return context.InstanceFactory.Create(new CreationContext(context, stateProvider ?? ParamsStateProvider.Empty));
            }

            if (Parent != null)
            {
                return Parent.Resolve(context, stateProvider);
            }

            throw new ContainerException($"Invalid container context.\nDetails:\n{this}");
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

        IDisposable IObservable<IRegistrationEvent>.Subscribe(IObserver<IRegistrationEvent> observer)
        {
            return _registrationSubject.Subscribe(observer);
        }

        bool IProvider<ICache<Type, IInstanceFactory>>.TryGet(out ICache<Type, IInstanceFactory> instance)
        {
            instance = _resolverFactoryCache;
            return true;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        bool IProvider<IFluent>.TryGet(out IFluent instance)
        {
            instance = _fluent;
            return true;
        }

        public override string ToString()
        {
            return $"{nameof(Container)} [Tag: {Tag ?? "null"}]{Environment.NewLine}{string.Join(Environment.NewLine, Registrations.Select(i => i.ToString()).ToArray())}";
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
                    _container._resolverContextCache.TryRemove(value.Key);
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
