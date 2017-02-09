namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;

    internal class InternalResourceStore: IInternalResourceStore
    {
        private readonly List<IDisposable> _resources = new List<IDisposable>();

        public void AddResource(IDisposable resource)
        {
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
