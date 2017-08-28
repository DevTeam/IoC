namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class Resolver<TContract>: IResolver<TContract>, IProvider, IProvider<TContract>, IFuncProvider
    {
        private readonly IResolving<IResolver> _resolving;

        public Resolver(ResolverContext context)
        {
            _resolving = context.Container.Resolve<IResolver>().Key(ExcludeContractKeys(context.Key)).Contract<TContract>();
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public TContract Resolve()
        {
            return (TContract)_resolving.Instance();
        }

        public bool TryGet(out object instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            if (_resolving.TryInstance(out object objInstance, stateProvider))
            {
                instance = objInstance;
                return true;
            }

            instance = default(object);
            return false;
        }

        public bool TryGet(out object instance, params object[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (_resolving.TryInstance(out object objInstance, state))
            {
                instance = objInstance;
                return true;
            }

            instance = default(object);
            return false;
        }

        public bool TryGet(out TContract instance)
        {
            if (TryGet(out object instanceObj))
            {
                instance = (TContract)instanceObj;
                return true;
            }

            instance = default(TContract);
            return false;
        }

        public virtual object GetFunc()
        {
            return new Func<TContract>(Resolve);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        protected IResolving<IResolver> CreateResolving()
        {
            return _resolving;
        }

        protected IStateProvider CreateStateProvider(params object[] state)
        {
            return ParamsStateProvider.Create(state);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private static IEnumerable<IKey> ExcludeContractKeys(IKey key)
        {
#if DEBUG
            if (key == null) throw new ArgumentNullException(nameof(key));
#endif
            switch (key)
            {
                case IContractKey _:
                    yield break;

                case IStateKey stateKey:
                    yield return stateKey;
                    yield break;

                case ITagKey tagKey:
                    yield return tagKey;
                    yield break;

                case ICompositeKey compositeKey:
#if NET35
                    foreach (var subKey in compositeKey.StateKeys.Cast<IKey>().Concat(compositeKey.TagKeys.Cast<IKey>()))
#else
                    foreach (var subKey in compositeKey.StateKeys.Concat(compositeKey.TagKeys.Cast<IKey>()))
#endif
                    {
                        yield return subKey;
                    }
                    yield break;
            }
        }
    }
}
