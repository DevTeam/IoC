namespace DevTeam.IoC.Tests
{
    using System.Linq;
    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class CacheTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void ShouldSetAndGetValue()
        {
            // Given
            var cache = CreateInstance();
            cache.Set(0, "abc");

            // When
            string str;
            var result = cache.TryGet(0, out str);

            // Then
            cache.Count.ShouldBe(1);
            result.ShouldBeTrue();
            str.ShouldBe("abc");
        }

        [Test]
        public void ShouldRemove()
        {
            // Given
            var cache = CreateInstance();
            cache.Set(0, "abc");
            cache.Set(1, "zyx");

            // When
            var result = cache.TryRemove(1);

            // Then
            cache.Count.ShouldBe(1);
            result.ShouldBeTrue();
            string str;
            cache.TryGet(0, out str).ShouldBeTrue();
        }

        [Test]
        public void ShouldNotGetAndClearWhenRefIsNotAlive()
        {
            // Given
            var cache = CreateInstance();
            cache.Set(0, "abc");
            cache.Set(1, "zyx");

            // When
            cache.Clear();
            string str;
            var result = cache.TryGet(0, out str);

            // Then
            result.ShouldBeFalse();
            cache.Count.ShouldBe(1);
        }

        private MyCache CreateInstance()
        {
            return new MyCache();
        }

        private class MyCache: Cache<int, string>
        {
            public void Clear()
            {
                foreach (var value in Values.Cast<MyWeakReference<string>>())
                {
                    value.Clear();
                }
            }

            protected override IWeakReference<string> CreateWeakReference(string value)
            {
                return new MyWeakReference<string>(value);
            }
        }

        private class MyWeakReference<T> : IWeakReference<T>
            where T: class
        {
            private readonly T _value;
            private bool _hasValue;

            public MyWeakReference(T value)
            {
                _value = value;
                _hasValue = true;
            }

            public bool TryGetTarget(out T target)
            {
                if (!_hasValue)
                {
                    target = default(T);
                    return false;
                }

                target = _value;
                return true;
            }

            public void Clear()
            {
                _hasValue = false;
            }
        }
    }
}
