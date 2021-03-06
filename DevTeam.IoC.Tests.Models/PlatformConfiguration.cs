﻿namespace DevTeam.IoC.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class PlatformConfiguration: IConfiguration
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

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            yield return container.Feature(Wellknown.Feature.Default);
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (_trace)
            {
                yield return container
                    .Register()
                    .Lifetime(Wellknown.Lifetime.Singleton)
                    .Contract<ITrace>()
                    .Autowiring<Trace>();
            }

            yield return container
                .Register()
                .Lifetime(Wellknown.Lifetime.Singleton)
                .Contract<IConsole>()
                .Autowiring<Console>();

            yield return container
                .Register()
                .Lifetime(Wellknown.Lifetime.Singleton)
                .Contract<ITimer>()
                .Contract<ITimerManager>()
                .Autowiring<Timer>();

            yield return container
                .Register()
                .Autowiring<Log>();
        }
    }
}
