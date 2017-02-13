namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    public class Container : IContainer
    {
        private static readonly ICompositeKey CacheKey = RootConfiguration.KeyFactory.CreateCompositeKey(Enumerable.Repeat(RootConfiguration.KeyFactory.CreateContractKey(typeof(ICache<ICompositeKey, RegistrationItem>), true), 1));
        private readonly IDisposable _rootConfigurationRegistration;
        private readonly IDisposable _configurationRegistration;
        private readonly IContainer _parentContainer;
        private readonly Dictionary<IEqualityComparer<ICompositeKey>, Dictionary<ICompositeKey, RegistrationItem>> _registrations = new Dictionary<IEqualityComparer<ICompositeKey>, Dictionary<ICompositeKey, RegistrationItem>>();
        private readonly Dictionary<Type, object> _subjects = new Dictionary<Type, object>();
        private ICache<ICompositeKey, RegistrationItem> _cache;

        public Container([CanBeNull] object tag = null, [CanBeNull] IContainer parentContainer = null)
        {
            _parentContainer = parentContainer;
            Tag = tag;

            if (_parentContainer == null)
            {
                _rootConfigurationRegistration = new CompositeDisposable(RootConfiguration.Shared.Apply(this));
            }

            _configurationRegistration = new CompositeDisposable(ContainerConfiguration.Shared.Apply(this));
        }

        public object Tag { get; }

        public IEnumerable<ICompositeKey> Registrations => _parentContainer == null ? _registrations.SelectMany(i => i.Value).Select(i => i.Key): _registrations.SelectMany(i => i.Value).Select(i => i.Key).Concat(_parentContainer.Registrations);

        private object LockObject => _registrations;

        public IObservable<T> GetEventSource<T>() where T : IEvent
        {
            return GetOrCreateSubject<T>();
        }

        public IRegistryContext CreateContext(IEnumerable<ICompositeKey> keys, IResolverFactory factory, IEnumerable<IExtension> extensions)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (extensions == null) throw new ArgumentNullException(nameof(extensions));
            var keysArr = keys.ToArray();
            if (keysArr.Length == 0) throw new ArgumentException("Value cannot be an empty enumerable.", nameof(keys));

            lock (LockObject)
            {
                return new RegistryContext(this, _parentContainer, keysArr, factory, extensions);
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
                    if (_parentContainer != null)
                    {
                        IResolverContext resolverContext;
                        if (_parentContainer.TryCreateContext(StaticContractKey<IRegistry>.Shared, out resolverContext,
                            EmptyStateProvider.Shared))
                        {
                            var parentRegistry = (IRegistry) _parentContainer.Resolve(resolverContext);
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
                    ICache<ICompositeKey, RegistrationItem> cache;
                    var hasCache = TryGetCache(out cache);
                    Subject<IEventRegistration> eventRegistrationSubject;
                    var hasEventRegistrationSubject = TryGetSubject(out eventRegistrationSubject);
                    foreach (var key in context.Keys)
                    {
                        var currentRegistration = newRegistration;

                        if (hasEventRegistrationSubject)
                        {
                            eventRegistrationSubject.OnNext(new EventRegistration(EventStage.Before, RegistrationAction.Add, key, currentRegistration.RegistryContext));
                        }

                        registrations.Add(key, currentRegistration);
                        resources.Add(
                            new Disposable(() =>
                            {
                                if (hasEventRegistrationSubject)
                                {
                                    eventRegistrationSubject.OnNext(new EventRegistration(EventStage.Before, RegistrationAction.Remove, key, currentRegistration.RegistryContext));
                                }

                                registrations.Remove(key);

                                if (hasEventRegistrationSubject)
                                {
                                    eventRegistrationSubject.OnNext(new EventRegistration(EventStage.After, RegistrationAction.Remove, key, currentRegistration.RegistryContext));
                                }
                            },
                            this));

                        if (hasCache)
                        {
                            cache.TryRemove(key);
                        }

                        if (hasEventRegistrationSubject)
                        {
                            eventRegistrationSubject.OnNext(new EventRegistration(EventStage.After, RegistrationAction.Add, key, currentRegistration.RegistryContext));
                        }
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
                ICache<ICompositeKey, RegistrationItem> cache;
                var hasCache = TryGetCache(out cache);
                if (TryCreateContextInternal(key, out resolverContext, hasCache ? cache : null, stateProvider))
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

            if (_parentContainer != null)
            {
                return _parentContainer.Resolve(context);
            }

            throw new InvalidOperationException("Invalid resolver context.");
        }

        public void Dispose()
        {
            Subject<IEventRegistration> eventRegistrationSubject;
            var hasEventRegistrationSubject = TryGetSubject(out eventRegistrationSubject);
            foreach (var registration in _registrations.ToList().SelectMany(i => i.Value).ToList())
            {
                if (hasEventRegistrationSubject)
                {
                    eventRegistrationSubject.OnNext(new EventRegistration(EventStage.Before, RegistrationAction.Remove, registration.Key, registration.Value.RegistryContext));
                }

                registration.Value.Dispose();

                if (hasEventRegistrationSubject)
                {
                    eventRegistrationSubject.OnNext(new EventRegistration(EventStage.After, RegistrationAction.Remove, registration.Key, registration.Value.RegistryContext));
                }
            }

            _registrations.Clear();
            _configurationRegistration.Dispose();
            _rootConfigurationRegistration?.Dispose();
            if (hasEventRegistrationSubject)
            {
                eventRegistrationSubject.OnCompleted();
            }
        }

        public override string ToString()
        {
            return $"{nameof(Container)} [Tag: {Tag ?? "null"}]{Environment.NewLine}{string.Join(Environment.NewLine, Registrations)}";
        }

        private bool TryGetSubject<T>(out Subject<T> subject) where T : IEvent
        {
            object subjectObj;
            if (_subjects.TryGetValue(typeof(T), out subjectObj))
            {
                subject = (Subject<T>)subjectObj;
                return true;
            }

            subject = default(Subject<T>);
            return false;
        }

        private Subject<T> GetOrCreateSubject<T>() where T : IEvent
        {
            Subject<T> subject;
            if (TryGetSubject(out subject))
            {
                return subject;
            }

            Subject<T> newSubject;
            if (_parentContainer != null)
            {
                var parentSubscription = new IDisposable[1];
                newSubject = new Subject<T>(subscriptionCount =>
                {
                    if (subscriptionCount == 0)
                    {
                        parentSubscription[0].Dispose();
                    }
                });

                parentSubscription[0] = _parentContainer.GetEventSource<T>().Subscribe(newSubject);
            }
            else
            {
                newSubject = new Subject<T>();
            }

            _subjects.Add(typeof(T), newSubject);
            return newSubject;
        }

        private bool TryCreateContextInternal(ICompositeKey key, out IResolverContext resolverContext, ICache<ICompositeKey, RegistrationItem> cache, IStateProvider stateProvider = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            RegistrationItem registrationItem;
            if (cache != null && cache.TryGet(key, out registrationItem))
            {
                resolverContext = new ResolverContext(
                    this,
                    _parentContainer,
                    registrationItem.RegistryContext,
                    registrationItem.InstanceFactory,
                    key,
                    registrationItem.Key,
                    stateProvider);

                return true;
            }

            foreach (var registrations in _registrations)
            {
                if (!registrations.Value.TryGetValue(key, out registrationItem))
                {
                    continue;
                }

                resolverContext = new ResolverContext(
                    this,
                    _parentContainer,
                    registrationItem.RegistryContext,
                    registrationItem.InstanceFactory,
                    key,
                    registrationItem.Key,
                    stateProvider);

                IScope scope;
                if (!TryGetExtension(resolverContext.RegistryContext.Extensions, out scope) || scope.AllowsResolving(resolverContext))
                {
                    cache?.Set(key, registrationItem);
                    return true;
                }
            }

            if (_parentContainer != null && _parentContainer.TryCreateContext(key, out resolverContext, stateProvider))
            {
                resolverContext = new ResolverContext(
                    this,
                    _parentContainer,
                    resolverContext.RegistryContext,
                    resolverContext.InstanceFactory,
                    resolverContext.Key,
                    resolverContext.RegistrationKey,
                    resolverContext.StateProvider);

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

        private bool TryGetCache(out ICache<ICompositeKey, RegistrationItem> cache)
        {
            var hasCache = _cache != null;
            if (!hasCache)
            {
                hasCache = TryResolve(CacheKey, out _cache);
            }

            cache = _cache;
            return hasCache;
        }

        private bool TryResolve<TContract>(ICompositeKey key, out TContract instance)
        {
            lock (LockObject)
            {
                IResolverContext resolverContext;
                if (!TryCreateContextInternal(key, out resolverContext, null))
                {
                    instance = default(TContract);
                    return false;
                }

                instance = (TContract) Resolve(resolverContext);
                return true;
            }
        }
    }
}
