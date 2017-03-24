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
        public IConfiguring<T> Configure<T>(T resolver)
              where T : IContainer
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            return new Configuring<T>(resolver);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public IRegistration<T> Register<T>(T resolver)
              where T : IContainer
        {
            return new Registration<T>(this, resolver);
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
