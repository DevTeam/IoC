namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class InternalScope: IScope
    {
        public bool AllowsRegistration([NotNull] IRegistryContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }

        public bool AllowsResolving([NotNull] IResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Container == context.RegistryContext.Container;
        }
    }
}
