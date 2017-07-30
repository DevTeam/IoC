namespace DevTeam.IoC
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Contracts;

    internal sealed class FilteredKeyComparer: IKeyComparer
    {
        private readonly KeyFilterContext _keyFilterContext;

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public FilteredKeyComparer(KeyFilterContext keyFilterContext)
        {
#if DEBUG
            if (keyFilterContext == null) throw new ArgumentNullException(nameof(keyFilterContext));
#endif
            _keyFilterContext = keyFilterContext;
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public bool Equals(IKey x, IKey y)
        {
            _keyFilterContext.Activate();
            try
            {
                return object.Equals(x, y);
            }
            finally
            {
                _keyFilterContext.Deactivate();
            }
        }

#if !NET35 && !NET40
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public int GetHashCode(IKey obj)
        {
            _keyFilterContext.Activate();
            try
            {
                return obj.GetHashCode();
            }
            finally
            {
                _keyFilterContext.Deactivate();
            }
        }
    }
}
