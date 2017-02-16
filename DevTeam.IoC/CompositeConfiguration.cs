namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class CompositeConfiguration : IConfiguration
    {
        private readonly IEnumerable<IConfiguration> _configurations;

        public CompositeConfiguration([NotNull] IEnumerable<IConfiguration> configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            _configurations = configurations;
        }

        public IEnumerable<IConfiguration> GetDependencies<T>(T container) where T : IResolver, IRegistry
        {
            return _configurations;
        }

        public IEnumerable<IDisposable> Apply<T>(T container) where T : IResolver, IRegistry
        {
            yield break;
        }
    }
}