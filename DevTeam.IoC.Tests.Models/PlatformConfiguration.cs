﻿namespace DevTeam.IoC.Tests.Models
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

        public IEnumerable<IConfiguration> GetDependencies(IResolver resolver)
        {
            yield return resolver.Configuration(Wellknown.Features.All);
        }

        public IEnumerable<IDisposable> Apply(IResolver resolver)
        {
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
                .Contract<ILog>()
                .State<string>(0)
                .AsAutowiring<Log>();
        }
    }
}
