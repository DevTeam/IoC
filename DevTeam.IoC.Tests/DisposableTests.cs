namespace DevTeam.IoC.Tests
{
    using System;
    using NUnit.Framework;

    using Shouldly;

    [TestFixture]
    public class DisposableTests
    {
        [Test]
        public void ShouldInvokeActionWhenDispose()
        {
            // Given
            var actualActionInvoked = false;
            var instance = CreateInstance(() => actualActionInvoked = true);

            // When
            instance.Dispose();

            // Then
            actualActionInvoked.ShouldBe(true);
        }

        private static Disposable CreateInstance(Action disposableAction)
        {
            return new Disposable(disposableAction);
        }
    }
}
