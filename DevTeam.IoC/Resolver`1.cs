﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Contracts;

    internal class Resolver<TContract>: IResolver<TContract>, IProvider, IProvider<TContract>, IFuncProvider
    {
        private readonly IResolving<IResolver> _resolving;

        public Resolver(IResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            _resolving = context.Container.Resolve<IResolver>().Key(ExcludeContractKeys(context.Key)).Contract<TContract>();
        }

        public TContract Resolve()
        {
            return (TContract)_resolving.Instance();
        }

        public bool TryGet(out object instance, IStateProvider stateProvider)
        {
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
            object objInstance;
            if (_resolving.TryInstance(out objInstance, stateProvider))
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
            object objInstance;
            if (_resolving.TryInstance(out objInstance, state))
            {
                instance = objInstance;
                return true;
            }

            instance = default(object);
            return false;
        }

        public bool TryGet(out TContract instance)
        {
            object instanceObj;
            if (TryGet(out instanceObj))
            {
                instance = (TContract) instanceObj;
                return true;
            }

            instance = default(TContract);
            return false;
        }

        public virtual object GetFunc()
        {
            return new Func<TContract>(Resolve);
        }

        protected IResolving<IResolver> CreateResolving()
        {
            return _resolving;
        }

        protected IStateProvider CreateStateProvider(params object[] state)
        {
            return ParamsStateProvider.Create(state);
        }

        private static IEnumerable<IKey> ExcludeContractKeys(IKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            var contractKey = key as IContractKey;
            if (contractKey != null)
            {
                yield break;
            }

            var stateKey = key as IStateKey;
            if (stateKey != null)
            {
                yield return stateKey;
                yield break;
            }

            var tagKey = key as ITagKey;
            if (tagKey != null)
            {
                yield return tagKey;
                yield break;
            }

            var compositeKey = key as ICompositeKey;
            if (compositeKey != null)
            {
                foreach (var subKey in ((IEnumerable<IKey>)compositeKey.StateKeys).Concat(compositeKey.TagKeys))
                {
                    yield return subKey;
                }
            }
        }
    }
}
