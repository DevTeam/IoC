namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal class InternalResourceStore: IInternalResourceStore
    {
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        public void AddResource([NotNull] IDisposable resource)
        {
#if DEBUG
            if (resource == null) throw new ArgumentNullException(nameof(resource));
#endif
            _resources.Add(resource);
        }

        public void Dispose()
        {
            foreach (var resource in _resources)
            {
                resource.Dispose();
            }

            _resources.Clear();
        }
    }
}
