namespace ClassLibrary
{
    using System;

    // ReSharper disable once UnusedMember.Global
    internal sealed class HelloWorld : IHelloWorld
    {
        private readonly IConsole _console;

        public HelloWorld(IConsole console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void SayHello()
        {
            _console.WriteLine("Hello");
        }
    }
}