namespace ConsoleApp
{
    using System;
    using Contracts;

    // ReSharper disable once UnusedMember.Global
    internal class Console: IConsole
    {
        public void WriteLine(string line, Color color)
        {
            if (line == null) throw new ArgumentNullException(nameof(line));
            var previousForegroundColor = System.Console.ForegroundColor;
            try
            {
                switch (color)
                {
                    case Color.Log:
                        System.Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;

                    case Color.Error:
                        System.Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }
                
                System.Console.WriteLine(line);
            }
            finally
            {
                System.Console.ForegroundColor = previousForegroundColor;
            }
        }
    }
}
