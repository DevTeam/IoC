namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class SingletonBasedLifetime<T>: KeyBasedLifetime<T>
    {
        public SingletonBasedLifetime([NotNull] Func<ILifetimeContext, IResolverContext, T> keySelector)
            : base(keySelector)
        {
        }

        protected override ILifetime CreateBaseLifetime(IEnumerator<ILifetime> lifetimeEnumerator)
        {
            return new SingletonLifetime();
        }
    }
}
