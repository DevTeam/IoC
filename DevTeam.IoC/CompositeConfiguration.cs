namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class CompositeConfiguration<T> : IConfiguration where T : IResolver, IRegistry
    {
        private readonly IEnumerable<IConfiguration> _configurations;

        public CompositeConfiguration([NotNull] IEnumerable<IConfiguration> configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            _configurations = configurations;
        }

        public IEnumerable<IConfiguration> GetDependencies<T1>(T1 container) where T1 : IResolver, IRegistry
        {
            return _configurations;
        }

        public IEnumerable<IDisposable> Apply<T1>(T1 container) where T1 : IResolver, IRegistry
        {
            yield break;
        }
    }
}