namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal sealed class GlobalScope: IScope
    {
#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool AllowRegistration(IRegistryContext context, IContainer targetContainer)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
#endif
            return targetContainer.Parent == null;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool AllowResolving(IResolverContext context)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
#endif
            return true;
        }

        public override string ToString()
        {
            return "Global Scope";
        }
    }
}
