namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class FullFeature: IConfiguration
    {
        public static readonly IConfiguration Shared = new FullFeature();

        private FullFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return ChildrenContainersFeature.Shared;
            yield return ResolversFeature.Shared;
            yield return LifetimesFeature.Shared;
            yield return ScopesFeature.Shared;
            yield return KeyComparersFeature.Shared;
            yield return EnumerablesFeature.Shared;
            yield return TasksFeature.Shared;
            yield return CacheFeature.Shared;
            yield return DtoFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            yield break;
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
