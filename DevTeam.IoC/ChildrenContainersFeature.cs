namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class ChildrenContainersFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new ChildrenContainersFeature();

        private ChildrenContainersFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return LifetimesFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            yield return 
                resolver
                .Register()
                .Lifetime(Wellknown.Lifetimes.AutoDisposing)
                .Contract<IContainer>()
                .AsFactoryMethod(ctx => new Container(null, ctx.Container));

            yield return
                resolver
                .Register()
                .Lifetime(Wellknown.Lifetimes.AutoDisposing)
                .State(0, typeof(object))
                .Contract<IContainer>()
                .AsFactoryMethod(ctx => new Container(ctx.GetState<object>(0), ctx.Container));
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
