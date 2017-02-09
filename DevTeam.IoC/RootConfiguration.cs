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
        public static readonly IMetadataProvider MetadataProvider = new AutowiringMetadataProvider();
        private static readonly IInstanceFactoryProvider ExpressionInstanceFactoryProvider = new ExpressionInstanceFactoryProvider();
        private static readonly IEnumerable<ICompositeKey> ResolverKeys = LowLevelRegistration.CreateKeys<IResolver>();
        private static readonly IEnumerable<ICompositeKey> RegistryKeys = LowLevelRegistration.CreateKeys<IRegistry>();
        private static readonly IEnumerable<ICompositeKey> KeyFactoryKeys = LowLevelRegistration.CreateKeys<IKeyFactory>();
        private static readonly IEnumerable<ICompositeKey> FluentKeys = LowLevelRegistration.CreateKeys<IFluent>();
        private static readonly IEnumerable<ICompositeKey> InstanceFactoryProviderKeys = LowLevelRegistration.CreateKeys<IInstanceFactoryProvider>();
        private static readonly IEnumerable<ICompositeKey> ResolvingKeys = LowLevelRegistration.CreateKeys<IResolving>();
        private static readonly IEnumerable<ICompositeKey> RegistrationKeys = LowLevelRegistration.CreateKeys<IRegistration>();
        private static readonly IEnumerable<ICompositeKey> MetadataProviderKeys = LowLevelRegistration.CreateKeys<IMetadataProvider>();

        private RootConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            var container = resolver as Container;
            if (container == null) throw new ArgumentException(nameof(resolver));
            yield return LowLevelRegistration.RawRegister<IResolver>(container, ResolverKeys, ctx => ctx.Container);
            yield return LowLevelRegistration.RawRegister<IRegistry>(container, RegistryKeys, ctx => (Container)ctx.Container);
            yield return LowLevelRegistration.RawRegister<IKeyFactory>(container, KeyFactoryKeys, ctx => KeyFactory);
            yield return LowLevelRegistration.RawRegister<IFluent>(container, FluentKeys, ctx => new Fluent(ctx.Container));
            yield return LowLevelRegistration.RawRegister(container, InstanceFactoryProviderKeys, ctx => ExpressionInstanceFactoryProvider);
            yield return LowLevelRegistration.RawRegister<IResolving>(container, ResolvingKeys, ctx => new Resolving(new Fluent(ctx.Container), container));
            yield return LowLevelRegistration.RawRegister<IRegistration>(container, RegistrationKeys, ctx => new Registration(new Fluent(ctx.Container), container));
            yield return LowLevelRegistration.RawRegister(container, MetadataProviderKeys, ctx => MetadataProvider);

            yield return
                container
                    .Register()
                    .State<IEnumerable<IParameterMetadata>>(0)
                    .Contract<IMetadataProvider>()
                    .AsFactoryMethod(ctx =>
                    {
                        var constructorParams = ctx.GetState<IEnumerable<IParameterMetadata>>(0);
                        if (constructorParams == null)
                        {
                            throw new InvalidOperationException($"{nameof(constructorParams)} was not defined.");
                        }

                        return new ManualMetadataProvider(MetadataProvider, constructorParams);
                    });

            yield return
                container
                .Register()
                .State<Type>(0)
                .Contract<IResolverFactory>()
                .AsFactoryMethod(ctx =>
                {
                    var implementationType = (Type)ctx.StateProvider.GetState(ctx, new StateKey(0, typeof(Type)));
                    if (implementationType == null)
                    {
                        throw new InvalidOperationException($"Can not resolve {nameof(implementationType)}");
                    }

                    ICache<Type, IResolverFactory> factoryCache;
                    if (ctx.Container.TryResolve(out factoryCache))
                    {
                        IResolverFactory factory;
                        if (!factoryCache.TryGet(implementationType, out factory))
                        {
                            factory = new MetadataFactory(implementationType, ExpressionInstanceFactoryProvider, MetadataProvider);
                            factoryCache.Set(implementationType, factory);
                        }

                        return factory;
                    }

                    return new MetadataFactory(implementationType, ExpressionInstanceFactoryProvider, MetadataProvider);
                });

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Default)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => DefaultFeatures.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.ChildContainers)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => ChildContainersFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Lifetimes)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => LifetimesFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Scopes)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => ScopesFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.KeyComaprers)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => KeyComparersFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Enumerables)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => EnumerablesFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Resolvers)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => ResolversFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Cache)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => CacheFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Dto)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => DtoFeature.Shared);

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Tasks)
                .Contract<IConfiguration>()
                .AsFactoryMethod(ctx => TasksFeature.Shared);
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj != null && GetType() == obj.GetType();
        }
    }
}
