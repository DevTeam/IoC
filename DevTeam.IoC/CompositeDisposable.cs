namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class CompositeDisposable : IDisposable
    {
        private readonly IList<IDisposable> _configurations;

        public CompositeDisposable(IEnumerable<IDisposable> configurations)
        {
            if (configurations == null) throw new ArgumentNullException(nameof(configurations));
            _configurations = configurations.ToList();
        }

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