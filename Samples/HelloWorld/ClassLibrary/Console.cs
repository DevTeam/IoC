namespace ClassLibrary
{
    using System;

    // Has no any dependencies
    internal class Console : IConsole
    {
        public void WriteLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            System.Console.WriteLine(line);
        }
    }
}