﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal sealed class ScopesFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new ScopesFeature();

        private readonly InternalScope _internalScope = new InternalScope();
        private readonly GlobalScope _globalScope = new GlobalScope();

        private ScopesFeature()
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
            yield return
                container
                .Register()
                .Tag(Wellknown.Scope.Internal)
                .Contract<IScope>()
                .FactoryMethod(ctx => _internalScope);

            yield return
                container
                .Register()
                .Tag(Wellknown.Scope.Global)
                .Contract<IScope>()
                .FactoryMethod(ctx => _globalScope);
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
