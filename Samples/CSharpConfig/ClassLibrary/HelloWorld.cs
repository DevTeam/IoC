namespace ClassLibrary
{
    using System;

    public class HelloWorld : IHelloWorld
    {
        private readonly IConsole _console;

        public HelloWorld(IConsole console)
        {
            if (console == null) throw new ArgumentNullException(nameof(console));
            _console = console;
        }

        public void SayHello()
        {
            _console.WriteLine("Hello");
        }
    }
}