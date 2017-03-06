namespace DevTeam.IoC
{
    using System;
    using Contracts;

    internal class FilteredKeyComparer: IKeyComparer
    {
        private readonly KeyFilterContext _keyFilterContext;

        public FilteredKeyComparer(KeyFilterContext keyFilterContext)
        {
#if DEBUG
            if (keyFilterContext == null) throw new ArgumentNullException(nameof(keyFilterContext));
#endif
            _keyFilterContext = keyFilterContext;
        }

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
