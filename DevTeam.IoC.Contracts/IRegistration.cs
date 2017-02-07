namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IRegistration : IToken<IRegistration>
    {
        [NotNull]
        IDisposable AsFactoryMethod([NotNull] Func<IResolverContext, object> factoryMethod);

        [NotNull]
        IDisposable AsFactoryMethod<TImplementation>([NotNull] Func<IResolverContext, TImplementation> factoryMethod);

        [NotNull]
        IDisposable AsAutowiring([NotNull] Type implementationType, [CanBeNull] IMetadataProvider metadataProvider = null);

        [NotNull]
        IDisposable AsAutowiring<TImplementation>();

        [NotNull]
        IRegistration Lifetime(Wellknown.Lifetimes lifetime);

        [NotNull]
        IRegistration Lifetime([NotNull] ILifetime lifetime);

        [NotNull]
        IRegistration KeyComparer(Wellknown.KeyComparers keyComparer);

        [NotNull]
        IRegistration KeyComparer([NotNull] IKeyComparer keyComparer);

        [NotNull]
        IRegistration Scope([NotNull] IScope scope);

        IRegistration Scope(Wellknown.Scopes scope);
    }
}