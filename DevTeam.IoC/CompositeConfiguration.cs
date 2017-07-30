namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class CompositeConfiguration : IConfiguration
    {
        private readonly IEnumerable<IConfiguration> _configurations;

        public CompositeConfiguration([NotNull] IEnumerable<IConfiguration> configurations)
        {
            _configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));
        }

        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            return _configurations;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            yield break;
        }
    }
}