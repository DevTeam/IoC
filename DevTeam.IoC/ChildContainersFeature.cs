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

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return LifetimesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return 
                resolver
                .Register()
                .Lifetime(Wellknown.Lifetime.AutoDisposing)
                .Contract<IContainer>()
                .FactoryMethod(ctx => new Container(null, ctx.Container));

            yield return
                resolver
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
