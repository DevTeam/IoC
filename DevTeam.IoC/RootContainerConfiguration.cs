namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal sealed class RootContainerConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new RootContainerConfiguration();
        public static readonly KeyFactory KeyFactory = new KeyFactory(Reflection.Shared);

        private static readonly IMethodFactory ExpressionMethodFactory = new ExpressionMethodFactory();
        private static readonly IEnumerable<IKey> ReflectionKeys = LowLevelRegistration.CreateKeys<IReflection>();
        private static readonly IEnumerable<IKey> ResolverKeys = LowLevelRegistration.CreateKeys<IResolver>();
        private static readonly IEnumerable<IKey> RegistryKeys = LowLevelRegistration.CreateKeys<IRegistry>();
        private static readonly IEnumerable<IKey> KeyFactoryKeys = LowLevelRegistration.CreateKeys<IKeyFactory>();
        private static readonly IEnumerable<IKey> FluentKeys = LowLevelRegistration.CreateKeys<IFluent>();
        private static readonly IEnumerable<IKey> InstanceFactoryProviderKeys = LowLevelRegistration.CreateKeys<IMethodFactory>();
        private static readonly IEnumerable<IKey> ResolvingKeys = LowLevelRegistration.CreateKeys<IResolving<IResolver>>();
        private static readonly IEnumerable<IKey> RegistrationKeys = LowLevelRegistration.CreateKeys<IRegistration<IContainer>>();
        private static readonly IEnumerable<IKey> MetadataProviderKeys = LowLevelRegistration.CreateKeys<IMetadataProvider>();
        private static readonly IEnumerable<IKey> CreationContextKeys = LowLevelRegistration.CreateKeys<ICreationContext>();
        private static readonly IEnumerable<IKey> ResolverContextKeys = LowLevelRegistration.CreateKeys<IResolverContext>();
        private static readonly IEnumerable<IKey> RegistryContextKeys = LowLevelRegistration.CreateKeys<IRegistryContext>();
        private static readonly Dictionary<Wellknown.Feature, IConfiguration> Features = new Dictionary<Wellknown.Feature, IConfiguration>
        {
            {Wellknown.Feature.Default, DefaultFeatures.Shared},
            {Wellknown.Feature.ChildContainers, ChildContainersFeature.Shared},
            {Wellknown.Feature.Dto, DtoFeature.Shared},
            {Wellknown.Feature.Enumerables, EnumerablesFeature.Shared},
            {Wellknown.Feature.KeyComparers, KeyComparersFeature.Shared},
            {Wellknown.Feature.Lifetimes, LifetimesFeature.Shared},
            {Wellknown.Feature.Observables, ObservablesFeature.Shared},
            {Wellknown.Feature.Resolvers, ResolversFeature.Shared},
            {Wellknown.Feature.Scopes, ScopesFeature.Shared},
#if !NET35
            {Wellknown.Feature.Tasks, TasksFeature.Shared},
#endif
        };

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
            yield return LowLevelRegistration.RawRegister<IRegistry>(container, RegistryKeys, ctx => ctx.ResolverContext.Container);
            yield return LowLevelRegistration.RawRegister<IKeyFactory>(container, KeyFactoryKeys, ctx => KeyFactory);
            yield return LowLevelRegistration.RawRegister(container, FluentKeys, ctx => Fluent.Shared);
            yield return LowLevelRegistration.RawRegister(container, ReflectionKeys, ctx => Reflection.Shared);
            yield return LowLevelRegistration.RawRegister(container, InstanceFactoryProviderKeys, ctx => ExpressionMethodFactory);
            yield return LowLevelRegistration.RawRegister(typeof(IResolving<>), container, ResolvingKeys, ctx => new Resolving<IResolver>(container));
            yield return LowLevelRegistration.RawRegister(typeof(IRegistration<>), container, RegistrationKeys, ctx => new Registration<IContainer>(ctx.ResolverContext.Container.Resolve().Instance<IFluent>(), container));
            yield return LowLevelRegistration.RawRegister(container, CreationContextKeys, ctx => ctx);
            yield return LowLevelRegistration.RawRegister(container, ResolverContextKeys, ctx => ctx.ResolverContext);
            yield return LowLevelRegistration.RawRegister(container, RegistryContextKeys, ctx => ctx.ResolverContext.RegistryContext);

            var sharedAutowiringMetadataProvider = new AutowiringMetadataProvider(Reflection.Shared);
            yield return LowLevelRegistration.RawRegister(container, MetadataProviderKeys, ctx =>
            {
                var reflection = ctx.ResolverContext.Container.Resolve().Instance<IReflection>();
                return reflection == Reflection.Shared ? sharedAutowiringMetadataProvider : new AutowiringMetadataProvider(reflection);
            });

            yield return
                container
                    .Register()
                    .State<TypeMetadata>(0)
                    .Contract<IMetadataProvider>()
                    .FactoryMethod(ctx => new ManualMetadataProvider(
                        ctx.ResolverContext.Container.Resolve().Instance<IMetadataProvider>(),
                        ctx.ResolverContext.Container.Resolve().Instance<IReflection>(),
                        ctx.GetState<TypeMetadata>(0)));

            yield return
                container
                .Register()
                .State<Type>(0)
                .Contract<IInstanceFactory>()
                .FactoryMethod(ctx =>
                {
                    var implementationType = ctx.GetState<Type>(0);
                    if (!ctx.ResolverContext.Container.TryResolve(out ICache<Type, IInstanceFactory> factoryCache))
                    {
                        return new MetadataFactory(
                            implementationType,
                            ctx.ResolverContext.Container.Resolve().Instance<IMethodFactory>(),
                            ctx.ResolverContext.Container.Resolve().Instance<IMetadataProvider>(),
                            ctx.ResolverContext.Container.KeyFactory);
                    }

                    if (factoryCache.TryGet(implementationType, out IInstanceFactory factory))
                    {
                        return factory;
                    }

                    factory = new MetadataFactory(
                        implementationType,
                        ctx.ResolverContext.Container.Resolve().Instance<IMethodFactory>(),
                        ctx.ResolverContext.Container.Resolve().Instance<IMetadataProvider>(),
                        ctx.ResolverContext.Container.KeyFactory);
                    factoryCache.Set(implementationType, factory);
                    return factory;
                });

            yield return
               container
               .Register()
               .Contract<IConfiguration>()
               .State<Assembly>(0)
               .FactoryMethod(ctx => new ConfigurationFromAssembly(ctx.GetState<Assembly>(0)));

            yield return
                container
                .Register()
                .State<Type>(0)
                .State<string>(1)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => new ConfigurationFromSringData(ctx.ResolverContext.Container, ctx.GetState<Type>(0), ctx.GetState<string>(1)));

            foreach (var feature in Features)
            {
                yield return
                    container
                    .Register()
                    .Tag(feature.Key)
                    .Contract<IConfiguration>()
                    .FactoryMethod(ctx => feature.Value);
            }
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
