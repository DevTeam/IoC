namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class CustomContainer : IContainer, IProvider<IFluent>
    {
        private static readonly IFluent SharedFluent = Fluent.Shared;
        private static readonly IKeyFactory SharedKeyFactory = new KeyFactory(Reflection.Shared);

        public CustomContainer([NotNull] IContainer parent)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            Parent = parent;
        }

        public object Tag { get; }

        public IEnumerable<IKey> Registrations { get; }

        public IContainer Parent { get; }

        public IKeyFactory KeyFactory => SharedKeyFactory;

        public IRegistryContext CreateRegistryContext(IEnumerable<IKey> keys, IInstanceFactory factory, params IExtension[] extensions)
        {
            return Parent.CreateRegistryContext(keys, factory, extensions);
        }

        public bool TryRegister(IRegistryContext context, out IDisposable registration)
        {
            return Parent.TryRegister(context, out registration);
        }

        public bool TryCreateResolverContext(IKey key, out IResolverContext resolverContext, IContainer container = null)
        {
            return Parent.TryCreateResolverContext(key, out resolverContext, container);
        }

        public object Resolve(IResolverContext context, IStateProvider stateProvider = null)
        {
            return Parent.Resolve(context, stateProvider);
        }

        public void Dispose()
        {
        }

            
        public bool TryGet(out IFluent instance)
        {
            instance = SharedFluent;
            return true;
        }
    }
}