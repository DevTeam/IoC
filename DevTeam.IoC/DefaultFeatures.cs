﻿namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal sealed class DefaultFeatures: IConfiguration
    {
        public static readonly IConfiguration Shared = new DefaultFeatures();

        private DefaultFeatures()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return ChildContainersFeature.Shared;
            yield return ResolversFeature.Shared;
            yield return LifetimesFeature.Shared;
            yield return ScopesFeature.Shared;
            yield return KeyComparersFeature.Shared;
            yield return EnumerablesFeature.Shared;
#if !NET35
            yield return TasksFeature.Shared;
#endif
            yield return DtoFeature.Shared;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
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
