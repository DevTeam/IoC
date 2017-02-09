namespace ClassLibrary
{
    using System;
    using DevTeam.IoC.Contracts;

    // Has no any dependencies
    [Contract(typeof(IConsole))]
    internal class Console : IConsole
    {
        public void WriteLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            System.Console.WriteLine(line);
        }
    }
}