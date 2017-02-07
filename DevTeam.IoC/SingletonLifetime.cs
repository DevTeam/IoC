namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class SingletonLifetime: KeyBasedLifetime<ICompositeKey>
    {
        private static readonly IStateKey[] EmptyStateKeys = new IStateKey[0];
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

        private static ICompositeKey KeySelector(ILifetimeContext lifetimeContext, IResolverContext resolverContext)
        {
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            ITagKey registrationKey = new TagKey(resolverContext.RegistrationKey);
            var contractKeys = resolverContext.Key.ContractKeys.Select(i => i.GenericTypeArguments).SelectMany(i => i).Select(i => (IContractKey)new ContractKey(i, true)).ToArray();
            var tagKeys = Enumerable.Repeat(registrationKey, 1).ToArray();
            return new CompositeKey(contractKeys, tagKeys, EmptyStateKeys);
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
                    if (_instance == null)
                    {
                        _instance = _baseLifetime.Create(lifetimeContext, resolverContext, lifetimeEnumerator);
                    }

                    return _instance;
                }
            }
        }
    }
}
