namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal abstract class Token<T> : IToken<T>
    {
        private readonly IKeyFactory _keyFactory;
        private readonly IRegistry _registry;
        protected static readonly IContractKey[] EmptyContractKeys = new IContractKey[0];
        protected static readonly ITagKey[] EmptyTagKeys = new ITagKey[0];
        protected static readonly IStateKey[] EmptyStateKeys = new IStateKey[0];

        protected Token(IFluent fluent, IResolver resolver)
        {
            if (fluent == null) throw new ArgumentNullException(nameof(fluent));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            Fluent = fluent;
            Resolver = resolver;

            if (!resolver.TryResolve(out _keyFactory) || !resolver.TryResolve(out _registry))
            {
                throw new InvalidOperationException();
            }
        }

        protected IFluent Fluent { get; }

        protected IResolver Resolver { get; }

        internal IKeyFactory KeyFactory => _keyFactory;

        protected IRegistry Registry => _registry;

        public T Key(IEnumerable<IKey> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));

            foreach (var key in keys)
            {
                var contractKey = key as IContractKey;
                if (contractKey != null)
                {
                    AddContractKey(Enumerable.Repeat(contractKey, 1));
                }

                var stateKey = key as IStateKey;
                if (stateKey != null)
                {
                    AddStateKey(stateKey);
                }

                var tagKey = key as ITagKey;
                if (tagKey != null)
                {
                    AddTagKey(tagKey);
                }

                var compositeKey = key as ICompositeKey;
                if (compositeKey != null)
                {
                    AddCompositeKey(compositeKey);
                }
            }

            return (T)(object)this;
        }

        public T Key(params IKey[] keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            return Key((IEnumerable<IKey>)keys);
        }

        public abstract T Contract(params Type[] contractTypes);

        public T Contract<TContract>()
        {
            return Contract(typeof(TContract));
        }

        public T State(int index, Type stateType)
        {
            if (stateType == null) throw new ArgumentNullException(nameof(stateType));
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            AddStateKey(KeyFactory.CreateStateKey(index, stateType));
            return (T)(object)this;
        }

        public T State<TState>(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return State(index, typeof(TState));
        }

        public T Tag([NotNull] params object[] tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            foreach (var tag in tags)
            {
                AddTagKey(KeyFactory.CreateTagKey(tag));
            }

            return (T)(object)this;
        }

        protected abstract bool AddContractKey(IEnumerable<IContractKey> keys);

        protected abstract bool AddStateKey(IStateKey key);

        protected abstract bool AddTagKey(ITagKey key);

        protected abstract bool AddCompositeKey(ICompositeKey compositeKey);
    }
}