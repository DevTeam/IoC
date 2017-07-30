namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public interface IRegistration<out T> : IToken<IRegistration<T>>
          where T : IResolver
    {
        [NotNull]
        IRegistrationResult<T> FactoryMethod([NotNull] Func<ICreationContext, object> factoryMethod);

        [NotNull]
        IRegistrationResult<T> FactoryMethod<TImplementation>([NotNull] Func<ICreationContext, TImplementation> factoryMethod);

        [NotNull]
        IRegistrationResult<T> Autowiring([NotNull] Type implementationType, bool lazy = false, [CanBeNull] IMetadataProvider metadataProvider = null);

        [NotNull]
        IRegistrationResult<T> Autowiring<TImplementation>(bool lazy = false);

        [NotNull]
        IRegistrationResult<T> Autowiring([NotNull] Type contractType, [NotNull] Type implementationType, [NotNull] params object[] tags);

        [NotNull]
        IRegistrationResult<T> Autowiring<TContract, TImplementation>([NotNull] params object[] tags) where TImplementation : TContract;

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