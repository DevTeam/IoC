namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal static class LowLevelResolving
    {
        public static bool TryResolve<TContract>(this IResolver resolver, out TContract instance)
        {
#if DEBUG
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
#endif
            if (!resolver.TryCreateResolverContext(StaticContractKey<TContract>.Shared, out var resolverContext))
            {
                instance = default(TContract);
                return false;
            }

            instance = (TContract)resolver.Resolve(resolverContext);
            return true;
        }
    }
}