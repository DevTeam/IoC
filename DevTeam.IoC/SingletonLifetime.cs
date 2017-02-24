namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class SingletonLifetime: KeyBasedLifetime<SingletonLifetime.Key>
    {
        private static readonly ILifetime TransientLifetime = new TransientLifetime();
        private readonly Dictionary<ICompositeKey, object> _instances = new Dictionary<ICompositeKey, object>();

        public SingletonLifetime()
            : base(KeySelector)
        {
        }

        protected override ILifetime CreateBaseLifetime(IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (!lifetimeEnumerator.MoveNext())
            {
                throw new InvalidOperationException("Should have another one lifetime in the chain");
            }

            return new Lifetime(lifetimeEnumerator.Current);
        }

        private static Key KeySelector(ILifetimeContext lifetimeContext, IResolverContext resolverContext)
        {
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            var genericTypes = (resolverContext.Key as IContractKey)?.GenericTypeArguments ?? (resolverContext.Key as ICompositeKey)?.ContractKeys?.Select(i => i.GenericTypeArguments).SelectMany(i => i).ToArray();
            return new Key(genericTypes);
        }

        internal class Key
        {
            private readonly Type[] _types;
            private readonly int _hashCode;

            public Key(Type[] types)
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
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Key) obj);
            }

            protected bool Equals(Key other)
            {
                return _types.SequenceEqual(other._types);
            }
        }

        private class Lifetime: ILifetime
        {
            private readonly ILifetime _baseLifetime;
            private readonly object _lockObject = new object();
            private object _instance;

            public Lifetime(ILifetime baseLifetime)
            {
                _baseLifetime = baseLifetime;
            }

            public void Dispose()
            {
                _baseLifetime.Dispose();
            }

            public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
            {
                lock (_lockObject)
                {
                    if (_instance != null)
                    {
                        return _instance;
                    }

                    _instance = _baseLifetime.Create(lifetimeContext, resolverContext, lifetimeEnumerator);
                    return _instance;
                }
            }
        }
    }
}
