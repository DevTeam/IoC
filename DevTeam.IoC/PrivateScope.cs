namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class PrivateScope: IScope
    {
        public bool IsVisible => false;

        public bool AllowsRegistration(IRegistryContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }

        public bool AllowsResolving(IResolverContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            return true;
        }
    }
}
