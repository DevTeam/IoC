namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class Fluent : IFluent
    {
        public IConfiguring<T> Configure<T>(T resolver)
              where T : IResolver, IRegistry
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return new Configuring<T>(resolver);
        }

        public IRegistration<T> Register<T>(T resolver)
              where T : IResolver, IRegistry
        {
            return new Registration<T>(this, resolver);
        }

        public IResolving<T> Resolve<T>(T resolver)
              where T : IResolver
        {
            return new Resolving<T>(this, resolver);
        }
    }
}
