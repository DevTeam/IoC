namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class DefaultFeatures: IConfiguration
    {
        public static readonly IConfiguration Shared = new DefaultFeatures();

        private DefaultFeatures()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver, IDisposable
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return ChildContainersFeature.Shared;
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
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
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
