namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class RootConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new RootConfiguration();

        public static readonly KeyFactory KeyFactory = new KeyFactory();

        private static readonly IInstanceFactoryProvider ExpressionInstanceFactoryProvider = new ExpressionInstanceFactoryProvider();
        private static readonly IEnumerable<ICompositeKey> ResolverKeys = CreateKeys<IResolver>();
        private static readonly IEnumerable<ICompositeKey> RegistryKeys = CreateKeys<IRegistry>();
        private static readonly IEnumerable<ICompositeKey> KeyFactoryKeys = CreateKeys<IKeyFactory>();
        private static readonly IEnumerable<ICompositeKey> FluentKeys = CreateKeys<IFluent>();
        private static readonly IEnumerable<ICompositeKey> InstanceFactoryProviderKeys = CreateKeys<IInstanceFactoryProvider>();
        private static readonly IEnumerable<ICompositeKey> ResolvingKeys = CreateKeys<IResolving>();
        private static readonly IEnumerable<ICompositeKey> RegistrationKeys = CreateKeys<IRegistration>();

        private RootConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            var container = resolver as Container;
            if (container == null) throw new ArgumentException(nameof(resolver));

            yield return RawRegister<IResolver>(container, ResolverKeys, ctx => ctx.Container);
            yield return RawRegister<IRegistry>(container, RegistryKeys, ctx => (Container)ctx.Container);
            yield return RawRegister<IKeyFactory>(container, KeyFactoryKeys, ctx => KeyFactory);
            yield return RawRegister<IFluent>(container, FluentKeys, ctx => new Fluent(ctx.Container));
            yield return RawRegister(container, InstanceFactoryProviderKeys, ctx => ExpressionInstanceFactoryProvider);
            yield return RawRegister<IResolving>(container, ResolvingKeys, ctx => new Resolving(new Fluent(ctx.Container), container));
            yield return RawRegister<IRegistration>(container, RegistrationKeys, ctx => new Registration(new Fluent(ctx.Container), container));

            yield return
                container
                .Register()
                .State<Type>(0)
                .Contract<IResolverFactory>()
                .AsFactoryMethod(ctx =>
                {
                    var implementationType = (Type)ctx.StateProvider.GetState(ctx, new StateKey(0, typeof(Type)));
                    ICache<Type, IResolverFactory> factoryCache;
                    if (ctx.Container.TryResolve(out factoryCache))
                    {
                        IResolverFactory factory;
                        if (!factoryCache.TryGet(implementationType, out factory))
                        {
                            factory = new MetadataFactory(implementationType, ExpressionInstanceFactoryProvider);
                            factoryCache.Set(implementationType, factory);
                        }

                        return factory;
                    }

                    return new MetadataFactory(implementationType, ExpressionInstanceFactoryProvider);
                });

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.All)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => AllConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.ChildrenContainers)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => ChildrenContainersConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.Lifetimes)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => LifetimesConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.Scopes)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => ScopesConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.KeyComaprers)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => KeyComparersConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.Enumerables)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => EnumerablesConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.Resolvers)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => ResolversConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.Cache)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => CacheConfiguration.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Configurations.Dto)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => DtoConfiguration.Shared);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }

        private static IEnumerable<ICompositeKey> CreateKeys<TContract>()
        {
            var key = new CompositeKey(new[] { (IContractKey)new ContractKey(typeof(TContract), true), }, new ITagKey[0], new IStateKey[0]);
            return Enumerable.Repeat(key, 1).ToArray();
        }

        private static IDisposable RawRegister<TContract>(IRegistry registry, IEnumerable<ICompositeKey> keys, Func<IResolverContext, TContract> factoryMethod)
        {
            var registryContext = registry.CreateContext(keys, new MethodFactory<TContract>(factoryMethod), Enumerable.Empty<IExtension>());
            IDisposable disposable;
            registry.TryRegister(registryContext, out disposable);
            return disposable;
        }
    }
}
