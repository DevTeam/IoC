#if NET35
// ReSharper disable once CheckNamespace
namespace System
{
    using Diagnostics.CodeAnalysis;

    internal sealed class Lazy<T>
    {
        private readonly Func<T> _factory;
        private bool _created;
        private T _value;

        [SuppressMessage("ReSharper", "JoinNullCheckWithUsage")]
        public Lazy(Func<T> factory)
        {
#if DEBUG
            if (factory == null) throw new ArgumentNullException(nameof(factory));
#endif
            _factory = factory;
        }

        public T Value
        {
            get
            {
                if (_created)
                {
                    return _value;
                }

                _created = true;
                _value = _factory();
                return _value;
            }
        }
    }
}
#endif
