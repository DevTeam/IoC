namespace DevTeam.IoC.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class PlatformConfiguration: IConfiguration
    {
        private readonly bool _trace;

        public PlatformConfiguration(bool trace)
        {
            _trace = trace;
        }

        [Autowiring]
        public PlatformConfiguration()
            :this(true)
        {
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T resolver) where T : IResolver, IDisposable
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            yield return resolver.Feature(Wellknown.Features.Default);
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            if (_trace)
            {
                yield return resolver
                    .Register()
                    .Lifetime(Wellknown.Lifetimes.Singleton)
                    .Contract<ITrace>()
                    .AsAutowiring<Trace>();
            }

            yield return resolver
                .Register()
                .Lifetime(Wellknown.Lifetimes.Singleton)
                .Contract<IConsole>()
                .AsAutowiring<Console>();

            yield return resolver
                .Register()
                .Lifetime(Wellknown.Lifetimes.Singleton)
                .Contract<ITimer>()
                .Contract<ITimerManager>()
                .AsAutowiring<Timer>();

            yield return resolver
                .Register()
                .AsAutowiring<Log>();
        }
    }
}
