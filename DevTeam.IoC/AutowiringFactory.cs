namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Contracts;

    internal class AutowiringFactory: IResolverFactory
    {
        private readonly ParamInfo[] _params;
        private readonly IInstanceFactory _instanceFactory;

        public AutowiringFactory(Type implementationType, IInstanceFactoryProvider instanceFactoryProvider)
        {
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));
            if (instanceFactoryProvider == null) throw new ArgumentNullException(nameof(instanceFactoryProvider));

            Exception error;
            ConstructorInfo constructor;
            if (!TryGetConstructor(implementationType, out constructor, out error))
            {
                throw error;
            }

            var ctorParams = constructor.GetParameters();
            _params = new ParamInfo[ctorParams.Length];
            var stateIndex = 0;
            for (var paramIndex = 0; paramIndex < ctorParams.Length; paramIndex++)
            {
                var paramInfo = new ParamInfo(ctorParams[paramIndex], stateIndex);
                if (!paramInfo.IsDependency)
                {
                    stateIndex++;
                }

                _params[paramIndex] = paramInfo;
            }

            _instanceFactory = instanceFactoryProvider.GetFactory(constructor);
        }

        public object Create(IResolverContext resolverContext)
        {
            if (resolverContext == null) throw new ArgumentNullException(nameof(resolverContext));
            return _instanceFactory.Create(_params.Select(param => param.Resolve(resolverContext)).ToArray());
        }

        private static bool TryGetConstructor(Type implementationType, out ConstructorInfo constructor, out Exception error)
        {
            var implementationTypeInfo = implementationType.GetTypeInfo();
            var ctors = implementationTypeInfo.DeclaredConstructors.Where(i => i.IsPublic).ToArray();
            if (ctors.Length == 1)
            {
                constructor = implementationTypeInfo.DeclaredConstructors.First();
                error = default(Exception);
                return true;
            }

            try
            {
                var autowiringConstructor = (
                    from ctor in ctors
                    let autowiringAttribute = ctor.GetCustomAttribute<AutowiringAttribute>()
                    where autowiringAttribute != null
                    select ctor).SingleOrDefault();

                if (autowiringConstructor != null)
                {
                    constructor = autowiringConstructor;
                    error = default(Exception);
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
                error = new InvalidOperationException("Too many resolving constructors.");
                constructor = default(ConstructorInfo);
                return false;
            }

            error = new InvalidOperationException("Resolving constructor was not found.");
            constructor = default(ConstructorInfo);
            return false;
        }

        internal class ParamInfo
        {
            private readonly IKey[] _keys;
            private readonly StateKey _stateKey;
            private readonly object[] _state;

            public ParamInfo(ParameterInfo info, int stateIndex)
            {
                var contractAttributes = info.GetCustomAttributes<ContractAttribute>().ToArray();
                IEnumerable<IContractKey> contractKeys;
                if (contractAttributes.Length > 0)
                {
                    contractKeys = contractAttributes.SelectMany(i => i.ContractTypes).Select(type => (IContractKey)new ContractKey(type, true)).DefaultIfEmpty(new ContractKey(info.ParameterType, true));
                }
                else
                {
                    contractKeys = Enumerable.Empty<IContractKey>();
                }

                var tagKeys = info.GetCustomAttributes<TagAttribute>().SelectMany(i => i.Tags).Select(i => (IKey)new TagKey(i));
                var stateAttributes = info.GetCustomAttributes<StateAttribute>().OrderBy(i => i.Index).ToArray();
                _state = stateAttributes.Select(i => i.Value).ToArray();
                var stateKeys = stateAttributes.Select(i => (IKey)new StateKey(i.Index, i.StateType));

                _keys = contractKeys.Concat(tagKeys).Concat(stateKeys).ToArray();
                if (_keys.Any())
                {
                    IsDependency = true;
                }
                else
                {
                    _stateKey = new StateKey(stateIndex, info.ParameterType);
                }
            }

            public bool IsDependency { get; }

            public object Resolve(IResolverContext resolverContext)
            {
                if (IsDependency)
                {
                    return resolverContext.RegistryContext.Container.Resolve().Key(_keys).Instance(_state);
                }

                return resolverContext.StateProvider.GetState(resolverContext, _stateKey);
            }
        }
    }
}