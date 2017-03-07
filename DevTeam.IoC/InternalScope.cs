namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class InternalScope: IScope
    {
        public bool AllowRegistration(IRegistryContext context, IContainer targetContainer)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
#endif
            return true;
        }

        public bool AllowResolving(IResolverContext context)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
#endif
            return context.Container == context.RegistryContext.Container;
        }
    }
}
