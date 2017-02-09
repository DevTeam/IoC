namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IRegistration<T> : IToken<IRegistration<T>>
         where T : IResolver
    {
        [NotNull]
        IDisposable AsFactoryMethod([NotNull] Func<IResolverContext, object> factoryMethod);

        [NotNull]
        IConfiguring<T> ConfigureFactoryMethod([NotNull] Func<IResolverContext, object> factoryMethod);

        [NotNull]
        IDisposable AsFactoryMethod<TImplementation>([NotNull] Func<IResolverContext, TImplementation> factoryMethod);

        [NotNull]
        IConfiguring<T> ConfigureFactoryMethod<TImplementation>([NotNull] Func<IResolverContext, TImplementation> factoryMethod);

        [NotNull]
        IDisposable AsAutowiring([NotNull] Type implementationType, [CanBeNull] IMetadataProvider metadataProvider = null);

        [NotNull]
        IConfiguring<T> ConfigureAsAutowiring([NotNull] Type implementationType, [CanBeNull] IMetadataProvider metadataProvider = null);

        [NotNull]
        IDisposable AsAutowiring<TImplementation>();

        [NotNull]
        IConfiguring<T> ConfigureAsAutowiring<TImplementation>();

        [NotNull]
        IRegistration<T> Attributes([NotNull] Type implementationType);

        [NotNull]
        IRegistration<T> Attributes<TImplementation>();

        [NotNull]
        IRegistration<T> Lifetime(Wellknown.Lifetime lifetime);

        [NotNull]
        IRegistration<T> Lifetime([NotNull] ILifetime lifetime);

        [NotNull]
        IRegistration<T> KeyComparer(Wellknown.KeyComparer keyComparer);

        [NotNull]
        IRegistration<T> KeyComparer([NotNull] IKeyComparer keyComparer);

        [NotNull]
        IRegistration<T> Scope([NotNull] IScope scope);

        IRegistration<T> Scope(Wellknown.Scope scope);
    }
}