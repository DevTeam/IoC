namespace DevTeam.IoC.Tests
{
    using System;
    using Shouldly;
    using Xunit;

    public class DisposableTests
    {
        [Fact]
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
