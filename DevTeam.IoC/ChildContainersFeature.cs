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

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return LifetimesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return 
                container
                .Register()
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IContainer>()
                .FactoryMethod(ctx => new Container(ctx.Container, null, ctx))
                .Apply();

            yield return
                container
                .Register()
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .State(0, typeof(object))
                .Contract<IContainer>()
                .FactoryMethod(ctx => new Container(ctx.Container, ctx.GetState<object>(0), ctx))
                .Apply();
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
