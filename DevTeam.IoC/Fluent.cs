namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class Fluent : IFluent
    {
        public static readonly IFluent Shared = new Fluent();

        private Fluent()
        {
        }

        public IConfiguring<T> Configure<T>(T resolver)
              where T : IContainer
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return new Configuring<T>(resolver);
        }

        public IRegistration<T> Register<T>(T resolver)
              where T : IContainer
        {
            return new Registration<T>(this, resolver);
        }

        public IResolving<T> Resolve<T>(T resolver)
              where T : IResolver
        {
            return new Resolving<T>(resolver);
        }
    }
}
