namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;

    internal class CompositeDisposable : IDisposable
    {
        private readonly IList<IDisposable> _configurations;

        public CompositeDisposable([NotNull] IEnumerable<IDisposable> configurations)
        {
#if DEBUG
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
#endif
            _configurations = configurations.ToList();
        }

        // ReSharper disable once UnusedMember.Global
        internal int Count => _configurations.Count;

        public void Dispose()
        {
            foreach (var configuration in _configurations)
            {
                configuration.Dispose();
            }

            _configurations.Clear();
        }
    }
}