namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal sealed class Resolver<TState1, TState2, TContract> : Resolver<TContract>, IResolver<TState1, TState2, TContract>, IProvider<TState1, TState2, TContract>
    {
        private readonly IResolving<IResolver> _resolving;

        public Resolver(IResolverContext context)
            : base(context)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
#endif
            _resolving = CreateResolving().State<TState1>(0).State<TState2>(1);
        }

        public TContract Resolve(TState1 state1, TState2 state2)
        {
            return (TContract)_resolving.Instance(CreateStateProvider(state1, state2));
        }

        public bool TryGet(out TContract instance, TState1 state1, TState2 state2)
        {
            object objInstance;
            if (_resolving.TryInstance(out objInstance, state1, state2))
            {
                instance = (TContract)objInstance;
                return true;
            }

            instance = default(TContract);
            return false;
        }

        public override object GetFunc()
        {
            return new Func<TState1, TState2, TContract>(Resolve);
        }
    }
}
