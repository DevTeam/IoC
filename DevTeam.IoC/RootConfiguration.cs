namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal class RootConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new RootConfiguration();
        public static readonly KeyFactory KeyFactory = new KeyFactory();
        public static readonly Fluent Fluent = new Fluent();
        public static readonly IMetadataProvider MetadataProvider = new AutowiringMetadataProvider();
        private static readonly IInstanceFactoryProvider ExpressionInstanceFactoryProvider = new ExpressionInstanceFactoryProvider();
        private static readonly IEnumerable<ICompositeKey> ResolverKeys = LowLevelRegistration.CreateKeys<IResolver>();
        private static readonly IEnumerable<ICompositeKey> RegistryKeys = LowLevelRegistration.CreateKeys<IRegistry>();
        private static readonly IEnumerable<ICompositeKey> KeyFactoryKeys = LowLevelRegistration.CreateKeys<IKeyFactory>();
        private static readonly IEnumerable<ICompositeKey> FluentKeys = LowLevelRegistration.CreateKeys<IFluent>();
        private static readonly IEnumerable<ICompositeKey> InstanceFactoryProviderKeys = LowLevelRegistration.CreateKeys<IInstanceFactoryProvider>();
        private static readonly IEnumerable<ICompositeKey> ResolvingKeys = LowLevelRegistration.CreateKeys<IResolving<IResolver>>();
        private static readonly IEnumerable<ICompositeKey> RegistrationKeys = LowLevelRegistration.CreateKeys<IRegistration<IResolver>>();
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
            var registry = resolver as IRegistry;
            if (registry == null) throw new ArgumentException(nameof(registry));
            yield return LowLevelRegistration.RawRegister<IResolver>(registry, ResolverKeys, ctx => ctx.Container);
            yield return LowLevelRegistration.RawRegister<IRegistry>(registry, RegistryKeys, ctx => (Container)ctx.Container);
            yield return LowLevelRegistration.RawRegister<IKeyFactory>(registry, KeyFactoryKeys, ctx => KeyFactory);
            yield return LowLevelRegistration.RawRegister<IFluent>(registry, FluentKeys, ctx => Fluent);
            yield return LowLevelRegistration.RawRegister(registry, InstanceFactoryProviderKeys, ctx => ExpressionInstanceFactoryProvider);
            yield return LowLevelRegistration.RawRegister(typeof(IResolving<>), registry, ResolvingKeys, ctx => new Resolving<IResolver>(Fluent, resolver));
            yield return LowLevelRegistration.RawRegister(typeof(IRegistration<>), registry, RegistrationKeys, ctx => new Registration<IResolver>(Fluent, resolver));
            yield return LowLevelRegistration.RawRegister(registry, MetadataProviderKeys, ctx => MetadataProvider);

            yield return
                resolver
                    .Register()
                    .State<IEnumerable<IParameterMetadata>>(0)
                    .Contract<IMetadataProvider>()
                    .FactoryMethod(ctx => new ManualMetadataProvider(MetadataProvider, ctx.GetState<IEnumerable<IParameterMetadata>>(0)));

            yield return
                resolver
                .Register()
                .State<Type>(0)
                .Contract<IResolverFactory>()
                .FactoryMethod(ctx =>
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
               resolver
               .Register()
               .Contract<IConfiguration>()
               .State<Assembly>(0)
               .FactoryMethod(ctx => new ConfigurationFromAssembly(ctx.GetState<Assembly>(0)));

            yield return
                resolver
                .Register()
                .State<Type>(0)
                .State<string>(1)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => new ConfigurationFromDto(ctx.Container, ctx.GetState<Type>(0), ctx.GetState<string>(1)));

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Default)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => DefaultFeatures.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.ChildContainers)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => ChildContainersFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Lifetimes)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => LifetimesFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Scopes)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => ScopesFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.KeyComaprers)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => KeyComparersFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Enumerables)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => EnumerablesFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Resolvers)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => ResolversFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Cache)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => CacheFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Dto)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => DtoFeature.Shared);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Feature.Tasks)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => TasksFeature.Shared);
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
