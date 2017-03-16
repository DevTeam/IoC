namespace DevTeam.IoC
{
    using System;

    internal class KeyFilterContext
    {
        private static readonly KeyFilterContext DefaultContext = new KeyFilterContext(key => false);
        [ThreadStatic] private static KeyFilterContext _current;
        private KeyFilterContext _previousContext;

        public static KeyFilterContext Current
        {
            get { return _current ?? DefaultContext; }
            private set { _current = value; }
        }

        public KeyFilterContext(Predicate<Type> filter)
        {
            if (filter == null) throw new ArgumentNullException(nameof(filter));
            Filter = filter;
        }

        public Predicate<Type> Filter { get; }

        public void Activate()
        {
            _previousContext = Current;
            Current = this;
        }

        public void Deactivate()
        {
            Current = _previousContext;
        }
    }
}
