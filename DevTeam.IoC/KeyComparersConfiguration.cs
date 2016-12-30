namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    using Contracts;

    internal class KeyComparersConfiguration : IConfiguration
    {
        public static readonly IConfiguration Shared = new KeyComparersConfiguration();

        private static readonly FilteredKeyComparer AnyTagKeyComparer = new FilteredKeyComparer(new KeyFilterContext(type => type == typeof(ITagKey)));
        private static readonly FilteredKeyComparer AnyStateKeyComparer = new FilteredKeyComparer(new KeyFilterContext(type => type == typeof(IStateKey)));
        private static readonly FilteredKeyComparer AnyTagAnyStateKeyComparer = new FilteredKeyComparer(new KeyFilterContext(type => type == typeof(ITagKey) || type == typeof(IStateKey)));

        private KeyComparersConfiguration()
        {
        }

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            yield return 
                resolver
                .Register()
                .Tag(Wellknown.KeyComparers.AnyTag)
                .Contract<IKeyComparer>()
                .AsFactoryMethod(ctx => AnyTagKeyComparer);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.KeyComparers.AnyState)
                .Contract<IKeyComparer>()
                .AsFactoryMethod(ctx => AnyStateKeyComparer);

            yield return
                resolver
                .Register()
                .Tag(Wellknown.KeyComparers.AnyTagAnyState)
                .Contract<IKeyComparer>()
                .AsFactoryMethod(ctx => AnyTagAnyStateKeyComparer);
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