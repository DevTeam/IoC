namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class InternalScope: IScope
    {
        public bool AllowRegistration(IRegistryContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }

        public bool AllowResolving(IResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Container == context.RegistryContext.Container;
        }
    }
}
