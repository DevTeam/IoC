namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Contracts;

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    internal abstract class Token<TResolver, TToken> : IToken<TToken>
         where TResolver : IResolver
    {
        protected static readonly IContractKey[] EmptyContractKeys = new IContractKey[0];
        protected static readonly ITagKey[] EmptyTagKeys = new ITagKey[0];
        protected static readonly IStateKey[] EmptyStateKeys = new IStateKey[0];

        protected Token([NotNull] TResolver resolver)
        {
#if DEBUG
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
#endif

            if (resolver is IProvider<IFluent> fluentProvider && fluentProvider.TryGet(out IFluent fluent))
            {
                Fluent = fluent;
            }
            else
            {
                throw new InvalidOperationException($"{typeof(IProvider<IFluent>)} is not supported. Only \"{nameof(IContainer)}\" is supported.");
            }

            Resolver = resolver;
        }

        protected IFluent Fluent;

        protected TResolver Resolver;

        public TToken Key(IEnumerable<IKey> keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            foreach (var key in keys)
            {
                switch (key)
                {
                    case IContractKey contractKey:
                        AddContractKey(Enumerable.Repeat(contractKey, 1));
                        break;

                    case IStateKey stateKey:
                        AddStateKey(stateKey);
                        break;

                    case ITagKey tagKey:
                        AddTagKey(tagKey);
                        break;

                    case ICompositeKey compositeKey:
                        AddCompositeKey(compositeKey);
                        break;
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