﻿namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class Fluent : IFluent
    {
        private readonly IResolver _resolver;

        public Fluent(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            _resolver = resolver;
        }

        public bool TryGetRegistry(out IRegistry registry)
        {
            return _resolver.TryResolve(out registry);
        }

        public IConfiguring<T> Configure<T>(T resolver)
            where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return new Configuring<T>(resolver);
        }

        public IRegistration Register()
        {
            return new Registration(this, _resolver);
        }

        public IResolving Resolve()
        {
            return new Resolving(this, _resolver);
        }
    }
}
