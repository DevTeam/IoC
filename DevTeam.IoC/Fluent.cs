namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal sealed class Fluent : IFluent
    {
        public static readonly IFluent Shared = new Fluent();

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        private Fluent()
        {
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public IConfiguring<TContainer> Configure<TContainer>(TContainer container)
              where TContainer : IContainer
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            return new Configuring<TContainer>(container);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public IRegistration<T> Register<T>(T container)
              where T : IContainer
        {
            return new Registration<T>(this, container);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public IResolving<T> Resolve<T>(T resolver)
              where T : IResolver
        {
            return new Resolving<T>(resolver);
        }
    }
}
