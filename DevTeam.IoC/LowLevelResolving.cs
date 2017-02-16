namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal static class LowLevelResolving
    {
        public static bool TryResolve<TContract>(this IResolver resolver, out TContract instance)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            IResolverContext resolverContext;
            if (!resolver.TryCreateResolverContext(StaticContractKey<TContract>.Shared, out resolverContext))
            {
                instance = default(TContract);
                return false;
            }

            instance = (TContract)resolver.Resolve(resolverContext);
            return true;
        }
    }
}