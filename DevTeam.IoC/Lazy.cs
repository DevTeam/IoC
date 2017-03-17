#if NET35
// ReSharper disable once CheckNamespace
namespace System
{
    public class Lazy<T>
    {
        private readonly Func<T> _factory;
        private bool _created;
        private T _value;

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
                if (!_created)
                {
                    _created = true;
                    _value = _factory();
                }

                return _value;
            }
        }
    }
}
#endif
