namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class GlobalScope: IScope
    {
        public bool AllowsRegistration(IRegistryContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Container.Parent == null;
        }

        public bool AllowsResolving(IResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
    }
}
