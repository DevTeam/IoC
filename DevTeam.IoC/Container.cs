namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    public class Container : IContainer
    {
        private readonly List<IDisposable> _resources = new List<IDisposable>();
        private readonly Dictionary<IEqualityComparer<ICompositeKey>, Dictionary<ICompositeKey, RegistrationItem>> _registrations = new Dictionary<IEqualityComparer<ICompositeKey>, Dictionary<ICompositeKey, RegistrationItem>>();
        private readonly Subject<IRegistrationEvent> _registrationSubject = new Subject<IRegistrationEvent>();
        private ICache<ICompositeKey, IResolverContext> _cache;

        public Container([CanBeNull] object tag = null)
        {
            Tag = tag;
            _resources.Add(new CompositeDisposable(RootConfiguration.Shared.Apply(this)));
            _resources.Add(new CompositeDisposable(ContainerConfiguration.Shared.Apply(this)));
            if (this.TryResolve(out _cache))
            {
                _resources.Add(Subscribe(new CacheTracker(_cache)));
            }
        }

        internal Container([NotNull] IContainer parentContainer, [CanBeNull] object tag = null, [CanBeNull] IResolverContext resolverContext = null)
        {
            Tag = tag;
            Parent = parentContainer;
            _resources.Add(new CompositeDisposable(ContainerConfiguration.Shared.Apply(this)));
            if (this.TryResolve(out _cache))
            {
                _resources.Add(Subscribe(new CacheTracker(_cache)));

                var parent = Parent;
                do
                {
                    _resources.Add(parent.Subscribe(new CacheTracker(_cache)));
                    parent = parent.Parent;
                }
                while (parent != null);
            }
        }

        public object Tag { get; }

        public IEnumerable<ICompositeKey> Registrations => _registrations.SelectMany(i => i.Value).Select(i => i.Key);

        public IContainer Parent { get; }

        private object LockObject => _registrations;

        public IRegistryContext CreateContext(IEnumerable<ICompositeKey> keys, IResolverFactory factory, IEnumerable<IExtension> extensions)
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
            var newRegistration = new RegistrationItem(context, new LifetimesFactory(context.Extensions.OfType<ILifetime>()), resources);

            lock (LockObject)
            {
                IScope scope;
                if (TryGetExtension(context.Extensions, out scope) && !scope.AllowsRegistration(context))
                {
                    if (Parent != null)
                    {
                        IResolverContext resolverContext;
                        if (Parent.TryCreateContext(StaticContractKey<IRegistry>.Shared, out resolverContext,
                            EmptyStateProvider.Shared))
                        {
                            var parentRegistry = (IRegistry) Parent.Resolve(resolverContext);
                            return parentRegistry.TryRegister(context, out registration);
                        }
                    }
                }

                IKeyComparer keyComparer;
                IEqualityComparer<ICompositeKey> comparer;
                if (TryGetExtension(context.Extensions, out keyComparer))
                {
                    comparer = keyComparer;
                }
                else
                {
                    comparer = EqualityComparer<IKey>.Default;
                }

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
                        var currentRegistration = newRegistration;
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
                    newRegistration.Dispose();
                    registration = default(IDisposable);
                    return false;
                }

                registration = newRegistration;
                return true;
            }
        }

        public IKeyFactory KeyFactory => RootConfiguration.KeyFactory;

        public bool TryCreateContext(ICompositeKey key, out IResolverContext resolverContext, IStateProvider stateProvider = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            lock (LockObject)
            {
                if (TryCreateContextInternal(key, out resolverContext, stateProvider))
                {
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

            throw new InvalidOperationException("Invalid resolver context.");
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

        private bool TryCreateContextInternal(
            ICompositeKey key,
            out IResolverContext resolverContext,
            IStateProvider stateProvider = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

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
                    this,
                    registrationItem.RegistryContext,
                    registrationItem.InstanceFactory,
                    key,
                    registrationItem.Key,
                    stateProvider);

                IScope scope;
                if (!TryGetExtension(resolverContext.RegistryContext.Extensions, out scope) || scope.AllowsResolving(resolverContext))
                {
                    _cache?.Set(key, resolverContext);
                    return true;
                }
            }

            var parent = Parent;
            while (parent != null && !parent.Registrations.Any())
            {
                parent = parent.Parent;
            }

            if (parent != null && parent.TryCreateContext(key, out resolverContext, stateProvider))
            {
                resolverContext = new ResolverContext(
                    this,
                    resolverContext.RegistryContext,
                    resolverContext.InstanceFactory,
                    resolverContext.Key,
                    resolverContext.RegistrationKey,
                    resolverContext.StateProvider);

                _cache?.Set(key, resolverContext);
                return true;
            }

            resolverContext = default(IResolverContext);
            return false;
        }

        private static bool TryGetExtension<TContract>(IEnumerable<IExtension> extensions, out TContract instance)
            where TContract: class, IExtension
        {
            instance = extensions.OfType<TContract>().SingleOrDefault();
            return instance != default(TContract);
        }
    }
}
