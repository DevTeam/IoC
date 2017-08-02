namespace DevTeam.IoC
{
    using System;

    using Contracts;

    internal sealed class InternalScope: IScope
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
            return true;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool AllowResolving(IResolverContext context)
        {
#if DEBUG
            if (context == null) throw new ArgumentNullException(nameof(context));
#endif
            return context.Container == context.RegistryContext.Container;
        }

        public override string ToString()
        {
            return "Internal Scope";
        }
    }
}
