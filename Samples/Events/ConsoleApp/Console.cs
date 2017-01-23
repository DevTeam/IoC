namespace ConsoleApp
{
    using System;
    using Contracts;

    internal class Console: IConsole
    {
        public void WriteLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            System.Console.WriteLine(line);
        }
    }
}
