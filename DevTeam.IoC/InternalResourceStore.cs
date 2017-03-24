namespace DevTeam.IoC
{
    using System;
    using System.Collections.Generic;
    using Contracts;

    internal sealed class InternalResourceStore: IInternalResourceStore
    {
        private readonly List<IDisposable> _resources = new List<IDisposable>();

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public void AddResource([NotNull] IDisposable resource)
        {
#if DEBUG
            if (resource == null) throw new ArgumentNullException(nameof(resource));
#endif
            _resources.Add(resource);
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
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
