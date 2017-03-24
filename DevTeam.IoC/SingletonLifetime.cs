namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                throw new InvalidOperationException("Should have another one lifetime in the chain");
            }

            return new Lifetime(lifetimeEnumerator.Current);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static Key KeySelector(ILifetimeContext lifetimeContext, ICreationContext creationContext)
        {
#if DEBUG
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (creationContext == null) throw new ArgumentNullException(nameof(creationContext));
#endif
            var genericTypes = (creationContext.ResolverContext.Key as IContractKey)?.GenericTypeArguments ?? (creationContext.ResolverContext.Key as ICompositeKey)?.ContractKeys?.FirstOrDefault(i => i.GenericTypeArguments.Length > 0)?.GenericTypeArguments;
            return genericTypes != null ? new Key(genericTypes) : Key.Empty;
        }

        internal sealed class Key
        {
            public static readonly Key Empty = new Key();
            private readonly Type[] _types;
            private readonly int _hashCode;

            public Key(params Type[] types)
            {
                _types = types;
                _hashCode = 1;
                foreach (var type in _types)
                {
                    unchecked
                    {
                        _hashCode = (type.GetHashCode() * 397) ^ _hashCode;
                    }
                }
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }

            public override bool Equals(object obj)
            {
#if DEBUG
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
#endif
                return Equals((Key) obj);
            }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
            private bool Equals(Key other)
            {
                return _types.SequenceEqual(other._types);
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
            public object Create(ILifetimeContext lifetimeContext, ICreationContext creationContext, IEnumerator<ILifetime> lifetimeEnumerator)
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
