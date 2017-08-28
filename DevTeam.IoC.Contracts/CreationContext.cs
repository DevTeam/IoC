namespace DevTeam.IoC.Contracts
{
    using System;

    [PublicAPI]
    public struct CreationContext
    {
        public CreationContext(ResolverContext resolverContext, [NotNull] IStateProvider stateProvider)
        {
#if DEBUG
            if (stateProvider == null) throw new ArgumentNullException(nameof(stateProvider));
#endif
            ResolverContext = resolverContext;
            StateProvider = stateProvider;
        }

        public readonly ResolverContext ResolverContext;

        public readonly IStateProvider StateProvider;

        public override string ToString()
        {
            return $"{nameof(CreationContext)} [ResolverContext: {ResolverContext}, StateProvider: {StateProvider}]";
        }
    }
}
