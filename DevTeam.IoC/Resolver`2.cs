﻿namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal sealed class Resolver<TState1, TContract> : Resolver<TContract>, IResolver<TState1, TContract>, IProvider<TState1, TContract>
    {
        private readonly IResolving<IResolver> _resolving;

        public Resolver(ResolverContext context)
            : base(context)
        {
            _resolving = CreateResolving().State<TState1>(0);
        }

        public TContract Resolve(TState1 state1)
        {
            return (TContract)_resolving.Instance(CreateStateProvider(state1));
        }

        public bool TryGet(out TContract instance, TState1 state1)
        {
            if (_resolving.TryInstance(out object objInstance, state1))
            {
                instance = (TContract)objInstance;
                return true;
            }

            instance = default(TContract);
            return false;
        }

        public override object GetFunc()
        {
            return new Func<TState1, TContract>(Resolve);
        }
    }
}
