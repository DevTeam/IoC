namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal abstract class Token<T, TToken> : IToken<TToken>
         where T : IResolver
    {
        protected static readonly IContractKey[] EmptyContractKeys = new IContractKey[0];
        protected static readonly ITagKey[] EmptyTagKeys = new ITagKey[0];
        protected static readonly IStateKey[] EmptyStateKeys = new IStateKey[0];

        protected Token([NotNull] T container)
        {
#if DEBUG
            if (container == null) throw new ArgumentNullException(nameof(container));
#endif
            Fluent = container.Fluent();
            Resolver = container;
        }

        protected IFluent Fluent;

        protected T Resolver;

        public TToken Key(IEnumerable<IKey> keys)
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

            return (TToken)(object)this;
        }

        public abstract TToken Contract(params Type[] contractTypes);

        public TToken Contract<TContract>()
        {
            return Contract(typeof(TContract));
        }

        public abstract TToken State(int index, Type stateType);

        public TToken State<TState>(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return State(index, typeof(TState));
        }

        public TToken Tag([NotNull] params object[] tags)
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            foreach (var tag in tags)
            {
                AddTagKey(Resolver.KeyFactory.CreateTagKey(tag));
            }

            return (TToken)(object)this;
        }

        protected abstract bool AddContractKey(IEnumerable<IContractKey> keys);

        protected abstract bool AddStateKey(IStateKey key);

        protected abstract bool AddTagKey(ITagKey key);

        protected abstract bool AddCompositeKey(ICompositeKey compositeKey);
    }
}