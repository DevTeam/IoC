namespace DevTeam.IoC
{
    internal struct WeakReference<T> : IWeakReference<T>
        where T : class
    {
        private readonly System.WeakReference<T> _weakReference;

        public WeakReference(T target)
        {
            _weakReference = new System.WeakReference<T>(target);
        }

        public bool TryGetTarget(out T target)
        {
            return _weakReference.TryGetTarget(out target);
        }
    }
}
