namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class SingletonBasedLifetime<T>: KeyBasedLifetime<T>
    {
        public SingletonBasedLifetime([NotNull] Func<ILifetimeContext, ICreationContext, T> keySelector)
            : base(keySelector)
        {
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        protected override ILifetime CreateBaseLifetime(IEnumerator<ILifetime> lifetimeEnumerator)
        {
            return new SingletonLifetime();
        }
    }
}
