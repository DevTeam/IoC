namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal class RootContainerConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new RootContainerConfiguration();
        public static readonly KeyFactory KeyFactory = new KeyFactory();
        public static readonly IMetadataProvider MetadataProvider = new AutowiringMetadataProvider();
        public static readonly Fluent Fluent = new Fluent();
        private static readonly IInstanceFactoryProvider ExpressionInstanceFactoryProvider = new ExpressionInstanceFactoryProvider();
        private static readonly IEnumerable<IKey> ResolverKeys = LowLevelRegistration.CreateKeys<IResolver>();
        private static readonly IEnumerable<IKey> RegistryKeys = LowLevelRegistration.CreateKeys<IRegistry>();
        private static readonly IEnumerable<IKey> KeyFactoryKeys = LowLevelRegistration.CreateKeys<IKeyFactory>();
        private static readonly IEnumerable<IKey> FluentKeys = LowLevelRegistration.CreateKeys<IFluent>();
        private static readonly IEnumerable<IKey> InstanceFactoryProviderKeys = LowLevelRegistration.CreateKeys<IInstanceFactoryProvider>();
        private static readonly IEnumerable<IKey> ResolvingKeys = LowLevelRegistration.CreateKeys<IResolving<IResolver>>();
        private static readonly IEnumerable<IKey> RegistrationKeys = LowLevelRegistration.CreateKeys<IRegistration<IContainer>>();
        private static readonly IEnumerable<IKey> MetadataProviderKeys = LowLevelRegistration.CreateKeys<IMetadataProvider>();

        private RootContainerConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return LowLevelRegistration.RawRegister<IResolver>(container, ResolverKeys, ctx => ctx.ResolverContext.Container);
            yield return LowLevelRegistration.RawRegister<IRegistry>(container, RegistryKeys, ctx => (Container)ctx.ResolverContext.Container);
            yield return LowLevelRegistration.RawRegister<IKeyFactory>(container, KeyFactoryKeys, ctx => KeyFactory);
            yield return LowLevelRegistration.RawRegister<IFluent>(container, FluentKeys, ctx => Fluent);
            yield return LowLevelRegistration.RawRegister(container, InstanceFactoryProviderKeys, ctx => ExpressionInstanceFactoryProvider);
            yield return LowLevelRegistration.RawRegister(typeof(IResolving<>), container, ResolvingKeys, ctx => new Resolving<IResolver>(container));
            yield return LowLevelRegistration.RawRegister(typeof(IRegistration<>), container, RegistrationKeys, ctx => new Registration<IContainer>(Fluent, container));
            yield return LowLevelRegistration.RawRegister(container, MetadataProviderKeys, ctx => MetadataProvider);

            yield return
                container
                    .Register()
                    .State<IEnumerable<IParameterMetadata>>(0)
                    .Contract<IMetadataProvider>()
                    .FactoryMethod(ctx => new ManualMetadataProvider(MetadataProvider, ctx.GetState<IEnumerable<IParameterMetadata>>(0)))
                    .Apply();

            yield return
                container
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
                    if (ctx.ResolverContext.Container.TryResolve(out factoryCache))
                    {
                        IResolverFactory factory;
                        if (!factoryCache.TryGet(implementationType, out factory))
                        {
                            factory = new MetadataFactory(implementationType, ExpressionInstanceFactoryProvider, MetadataProvider, ctx.ResolverContext.Container.KeyFactory);
                            factoryCache.Set(implementationType, factory);
                        }

                        return factory;
                    }

                    return new MetadataFactory(implementationType, ExpressionInstanceFactoryProvider, MetadataProvider, ctx.ResolverContext.Container.KeyFactory);
                })
                .Apply();

            yield return
               container
               .Register()
               .Contract<IConfiguration>()
               .State<Assembly>(0)
               .FactoryMethod(ctx => new ConfigurationFromAssembly(ctx.GetState<Assembly>(0)))
               .Apply();

            yield return
                container
                .Register()
                .State<Type>(0)
                .State<string>(1)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => new ConfigurationFromSringData(ctx.ResolverContext.Container, ctx.GetState<Type>(0), ctx.GetState<string>(1)))
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Default)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => DefaultFeatures.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.ChildContainers)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => ChildContainersFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Lifetimes)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => LifetimesFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Scopes)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => ScopesFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.KeyComaprers)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => KeyComparersFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Enumerables)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => EnumerablesFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Observables)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => ObservablesFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Resolvers)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => ResolversFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Dto)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => DtoFeature.Shared)
                .Apply();

            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Tasks)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => TasksFeature.Shared)
                .Apply();
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
