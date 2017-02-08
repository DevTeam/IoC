namespace DevTeam.IoC.Tests
{
    using System;
    using System.Collections.Generic;
    using Contracts;
    using Moq;

    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class CompositeDisposableTests
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void ShouldDisposable()
        {
            // Given
            var disposable1 =new Mock<IDisposable>();
            var disposable2 = new Mock<IDisposable>();
            var instance = CreateInstance(new []{ disposable1.Object, disposable2.Object });

            // When
            instance.Dispose();

            // Then
            instance.Count.ShouldBe(0);
            disposable1.Verify(i => i.Dispose(), Times.Once);
            disposable2.Verify(i => i.Dispose(), Times.Once);
        }

        [Test]
        public void ShouldDisposableWhenDisposeSeveralTimes()
        {
            // Given
            var disposable1 = new Mock<IDisposable>();
            var disposable2 = new Mock<IDisposable>();
            var instance = CreateInstance(new[] { disposable1.Object, disposable2.Object });

            // When
            instance.Dispose();
            instance.Dispose();
            instance.Dispose();

            // Then
            instance.Count.ShouldBe(0);
            disposable1.Verify(i => i.Dispose(), Times.Once);
            disposable2.Verify(i => i.Dispose(), Times.Once);
        }

        private CompositeDisposable CreateInstance([NotNull] IEnumerable<IDisposable> configurations)
        {
            return new CompositeDisposable(configurations);
        }
    }
}
