namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class SingletonLifetime: ILifetime
    {
        private static readonly IStateKey[] EmptyStateKeys = new IStateKey[0];
        private readonly Dictionary<ICompositeKey, object> _instances = new Dictionary<ICompositeKey, object>();

        internal int Count => _instances.Count;

        public object Create(ILifetimeContext lifetimeContext, IResolverContext resolverContext, IEnumerator<ILifetime> lifetimeEnumerator)
        {
            if (lifetimeContext == null) throw new ArgumentNullException(nameof(lifetimeContext));
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            if (lifetimeEnumerator == null) throw new ArgumentNullException(nameof(lifetimeEnumerator));
            ITagKey registrationKey = new TagKey(resolverContext.RegistrationKey);
            var contractKeys = resolverContext.Key.ContractKeys.Select(i => i.GenericTypeArguments).SelectMany(i => i).Select(i => (IContractKey)new ContractKey(i, true)).ToArray();
            var tagKeys = Enumerable.Repeat(registrationKey, 1).ToArray();
            var key = new CompositeKey(contractKeys, tagKeys, EmptyStateKeys);
            lock (_instances)
            {
                object instance;
                if (_instances.TryGetValue(key, out instance))
                {
                    return instance;
                }

                if (!lifetimeEnumerator.MoveNext())
                {
                    throw new InvalidOperationException();
                }

                instance = lifetimeEnumerator.Current.Create(lifetimeContext, resolverContext, lifetimeEnumerator);
                _instances.Add(key, instance);
                return instance;
            }
        }

        public void Dispose()
        {
            lock (_instances)
            {
                _instances.Clear();
            }
        }
    }
}
