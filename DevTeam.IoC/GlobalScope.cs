namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal class GlobalScope: IScope
    {
        public bool AllowRegistration(IRegistryContext context, IContainer targetContainer)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
#endif
            return targetContainer.Parent == null;
        }

        public bool AllowResolving(IResolverContext context)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
#endif
            return true;
        }
    }
}
