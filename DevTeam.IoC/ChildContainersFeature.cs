namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class ChildContainersFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new ChildContainersFeature();

        private ChildContainersFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return LifetimesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return 
                container
                .Register()
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IContainer>()
                .FactoryMethod(ctx => new Container(null, ctx.Container));

            yield return
                container
                .Register()
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .State(0, typeof(object))
                .Contract<IContainer>()
                .FactoryMethod(ctx => new Container(ctx.GetState<object>(0), ctx.Container));
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
