namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal sealed class InternalScope: IScope
    {
#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool AllowRegistration(RegistryContext context, IContainer targetContainer)
        {
#if DEBUG
            if (targetContainer == null) throw new ArgumentNullException(nameof(targetContainer));
#endif
            return true;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool AllowResolving(ResolverContext context)
        {
            return context.Container == context.RegistryContext.Container;
        }

        public override string ToString()
        {
            return "Internal Scope";
        }
    }
}
