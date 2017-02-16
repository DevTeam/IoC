namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IRegistration<T> : IToken<IRegistration<T>>
          where T : IContainer
    {
        [NotNull]
        IRegistrationResult<T> FactoryMethod([NotNull] Func<IResolverContext, object> factoryMethod);

        [NotNull]
        IRegistrationResult<T> FactoryMethod<TImplementation>([NotNull] Func<IResolverContext, TImplementation> factoryMethod);

        [NotNull]
        IRegistrationResult<T> Autowiring([NotNull] Type implementationType, [CanBeNull] IMetadataProvider metadataProvider = null);

        [NotNull]
        IRegistrationResult<T> Autowiring<TImplementation>();

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

        [NotNull]
        T ToSelf([NotNull] params IDisposable[] resource);
    }
}