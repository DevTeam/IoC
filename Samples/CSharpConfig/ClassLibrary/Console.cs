namespace ClassLibrary
{
    using System;

    // ReSharper disable once UnusedMember.Global
    public class Console : IConsole
    {
        public void WriteLine(string line)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            System.Console.WriteLine(line);
        }
    }
}