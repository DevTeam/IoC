namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class ScopesFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new ScopesFeature();

        private readonly InternalScope _internalScope = new InternalScope();
        private readonly GlobalScope _globalScope = new GlobalScope();

        private ScopesFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver, IDisposable
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return
                resolver
                .Register()
                .Tag(Wellknown.Scopes.Internal)
                .Contract<IScope>()
                .AsFactoryMethod(ctx => _internalScope);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.Scopes.Global)
                .Contract<IScope>()
                .AsFactoryMethod(ctx => _globalScope);
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
