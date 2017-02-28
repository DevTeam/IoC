namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class GlobalScope: IScope
    {
        public bool AllowRegistration(IRegistryContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return context.Container.Parent == null;
        }

        public bool AllowResolving(IResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
    }
}
