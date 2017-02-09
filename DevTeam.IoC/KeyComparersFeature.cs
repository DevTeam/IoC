namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class KeyComparersFeature : IConfiguration
    {
        public static readonly IConfiguration Shared = new KeyComparersFeature();

        private static readonly FilteredKeyComparer AnyTagKeyComparer = new FilteredKeyComparer(new KeyFilterContext(type => type == typeof(ITagKey)));
        private static readonly FilteredKeyComparer AnyStateKeyComparer = new FilteredKeyComparer(new KeyFilterContext(type => type == typeof(IStateKey)));
        private static readonly FilteredKeyComparer AnyTagAnyStateKeyComparer = new FilteredKeyComparer(new KeyFilterContext(type => type == typeof(ITagKey) || type == typeof(IStateKey)));

        private KeyComparersFeature()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return 
                resolver
                .Register()
                .Tag(Wellknown.KeyComparer.AnyTag)
                .Contract<IKeyComparer>()
                .FactoryMethod(ctx => AnyTagKeyComparer);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.KeyComparer.AnyState)
                .Contract<IKeyComparer>()
                .FactoryMethod(ctx => AnyStateKeyComparer);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.KeyComparer.AnyTagAnyState)
                .Contract<IKeyComparer>()
                .FactoryMethod(ctx => AnyTagAnyStateKeyComparer);
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