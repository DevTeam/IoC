namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class AllConfiguration: IConfiguration
    {
        public static readonly IConfiguration Shared = new AllConfiguration();

        private AllConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return ChildrenContainersConfiguration.Shared;
            yield return ResolversConfiguration.Shared;
            yield return LifetimesConfiguration.Shared;
            yield return ScopesConfiguration.Shared;
            yield return KeyComparersConfiguration.Shared;
            yield return EnumerablesConfiguration.Shared;
            yield return TasksConfiguration.Shared;
            yield return CacheConfiguration.Shared;
            yield return DtoConfiguration.Shared;
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
