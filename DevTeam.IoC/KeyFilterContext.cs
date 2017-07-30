namespace DevTeam.IoC
{
    using System;

    internal sealed class KeyFilterContext
    {
        private static readonly KeyFilterContext DefaultContext = new KeyFilterContext(key => false);
        [ThreadStatic] private static KeyFilterContext _current;
        private KeyFilterContext _previousContext;

        public static KeyFilterContext Current
        {
            get => _current ?? DefaultContext;
            private set => _current = value;
        }

        public KeyFilterContext(Predicate<Type> filter)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
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
