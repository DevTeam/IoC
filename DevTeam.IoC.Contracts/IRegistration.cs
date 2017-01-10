namespace DevTeam.IoC.Contracts
{
    using System;

    public interface IRegistration : IToken<IRegistration>
    {
        IDisposable AsFactoryMethod(Func<IResolverContext, object> factoryMethod);

        IDisposable AsFactoryMethod<TImplementation>(Func<IResolverContext, TImplementation> factoryMethod);

        IDisposable AsAutowiring(Type implementationType, IMetadataProvider metadataProvider = null);

        IDisposable AsAutowiring<TImplementation>();

        IRegistration Lifetime(Wellknown.Lifetimes lifetime);

        IRegistration Lifetime(ILifetime lifetime);

        IRegistration KeyComparer(Wellknown.KeyComparers keyComparer);

        IRegistration KeyComparer(IKeyComparer keyComparer);

        IRegistration Scope(IScope scope);

        IRegistration Scope(Wellknown.Scopes scope);
    }
}