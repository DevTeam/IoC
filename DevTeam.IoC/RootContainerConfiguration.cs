﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Contracts;

    internal class RootContainerConfiguration: IConfiguration
    {
        public static readonly IReflection Reflection = new Reflection();
        public static readonly IConfiguration Shared = new RootContainerConfiguration();
        public static readonly KeyFactory KeyFactory = new KeyFactory(Reflection);
        public static readonly IMetadataProvider MetadataProvider = new AutowiringMetadataProvider();
        public static readonly Fluent Fluent = new Fluent();
        private static readonly IMethodFactory ExpressionMethodFactory = new ExpressionMethodFactory();
        private static readonly IEnumerable<IKey> ReflectionKeys = LowLevelRegistration.CreateKeys<IReflection>(Reflection);
        private static readonly IEnumerable<IKey> ResolverKeys = LowLevelRegistration.CreateKeys<IResolver>(Reflection);
        private static readonly IEnumerable<IKey> RegistryKeys = LowLevelRegistration.CreateKeys<IRegistry>(Reflection);
        private static readonly IEnumerable<IKey> KeyFactoryKeys = LowLevelRegistration.CreateKeys<IKeyFactory>(Reflection);
        private static readonly IEnumerable<IKey> FluentKeys = LowLevelRegistration.CreateKeys<IFluent>(Reflection);
        private static readonly IEnumerable<IKey> InstanceFactoryProviderKeys = LowLevelRegistration.CreateKeys<IMethodFactory>(Reflection);
        private static readonly IEnumerable<IKey> ResolvingKeys = LowLevelRegistration.CreateKeys<IResolving<IResolver>>(Reflection);
        private static readonly IEnumerable<IKey> RegistrationKeys = LowLevelRegistration.CreateKeys<IRegistration<IContainer>>(Reflection);
        private static readonly IEnumerable<IKey> MetadataProviderKeys = LowLevelRegistration.CreateKeys<IMetadataProvider>(Reflection);

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
            yield return LowLevelRegistration.RawRegister(container, ReflectionKeys, ctx => Reflection);
            yield return LowLevelRegistration.RawRegister(container, InstanceFactoryProviderKeys, ctx => ExpressionMethodFactory);
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
                    var implementationType = ctx.GetState<Type>(0);
                    var reflection = ctx.ResolverContext.Container.Resolve().Instance<IReflection>();
                    if (!ctx.ResolverContext.Container.TryResolve(out ICache<Type, IResolverFactory> factoryCache))
                    {
                        return new MetadataFactory(reflection, implementationType, ExpressionMethodFactory, MetadataProvider, ctx.ResolverContext.Container.KeyFactory);
                    }

                    if (factoryCache.TryGet(implementationType, out IResolverFactory factory))
                    {
                        return factory;
                    }

                    factory = new MetadataFactory(reflection, implementationType, ExpressionMethodFactory, MetadataProvider, ctx.ResolverContext.Container.KeyFactory);
                    factoryCache.Set(implementationType, factory);
                    return factory;
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

#if !NET35
            yield return
                container
                .Register()
                .Tag(Wellknown.Feature.Tasks)
                .Contract<IConfiguration>()
                .FactoryMethod(ctx => TasksFeature.Shared)
                .Apply();
#endif
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
