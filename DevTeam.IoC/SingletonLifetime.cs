namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class SingletonLifetime: KeyBasedLifetime<SingletonLifetime.Key>
    {
        private static readonly ILifetime TransientLifetime = new TransientLifetime();
        private readonly Dictionary<ICompositeKey, object> _instances = new Dictionary<ICompositeKey, object>();

        public SingletonLifetime()
            : base(KeySelector)
        {
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        protected override ILifetime CreateBaseLifetime(IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (!lifetimeEnumerator.MoveNext())
            {
                throw new ContainerException("Should have another one lifetime in the chain.");
            }

            return new Lifetime(lifetimeEnumerator.Current);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static Key KeySelector(ILifetimeContext lifetimeContext, CreationContext creationContext)
        {
#if DEBUG
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
#endif
            var resolverContext = creationContext.ResolverContext;
            switch (resolverContext.Key)
            {
                case ICompositeKey compositeKey:
                    foreach (var key in compositeKey.ContractKeys)
                    {
                        var genericTypeArguments = key.GenericTypeArguments;
                        if (genericTypeArguments.Length > 0)
                        {
                            return new Key(resolverContext.RegistryContext.Id, genericTypeArguments);
                        }
                    }
                    return new Key(resolverContext.RegistryContext.Id);

                case IContractKey contractKey:
                    return new Key(resolverContext.RegistryContext.Id, contractKey.GenericTypeArguments);

                default:
                    throw new ContainerException($"Unknown key type {resolverContext.Key}");
            }
        }

        internal struct Key
        {
            private readonly long _registryContextId;
            [CanBeNull] private readonly Type[] _types;
            private readonly int _hashCode;

            public Key(long registryContextId, [CanBeNull] params Type[] types)
            {
                _registryContextId = registryContextId;
                _types = types;
                _hashCode = registryContextId.GetHashCode();
                if (_types != null)
                {
                    foreach (var type in _types)
                    {
                        unchecked
                        {
                            _hashCode = (type.GetHashCode() * 397) ^ _hashCode;
                        }
                    }
                }
            }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override int GetHashCode()
            {
                return _hashCode;
            }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public override bool Equals(object obj)
            {
#if DEBUG
                if (ReferenceEquals(null, obj)) return false;
                if (obj.GetType() != GetType()) return false;
#endif
                return Equals((Key) obj);
            }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            private bool Equals(Key other)
            {
                return _registryContextId == other._registryContextId
                    && (_types == other._types || (_types != null && other._types != null && Arrays.SequenceEqual(_types, other._types)));
            }
        }

        private sealed class Lifetime: ILifetime
        {
            private readonly ILifetime _baseLifetime;
            private readonly object _lockObject = new object();
            private object _instance;

            public Lifetime(ILifetime baseLifetime)
            {
                _baseLifetime = baseLifetime;
            }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public void Dispose()
            {
                _baseLifetime.Dispose();
            }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            public object Create(ILifetimeContext lifetimeContext, CreationContext creationContext, IEnumerator<ILifetime> lifetimeEnumerator)
            {
                lock (_lockObject)
                {
                    if (_instance != null)
                    {
                        return _instance;
                    }

                    _instance = _baseLifetime.Create(lifetimeContext, creationContext, lifetimeEnumerator);
                    return _instance;
                }
            }
        }
    }
}
