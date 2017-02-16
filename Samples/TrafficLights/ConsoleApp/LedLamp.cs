namespace ConsoleApp
{
    using System;
    using DevTeam.IoC.Contracts;

    internal class LedLamp : ILamp
    {
        private readonly ConsoleColor _color;

        public LedLamp([State] ConsoleColor color)
        {
            _color = color;
        }

        public bool IsActive
        {
            set
            {
                var color = Console.BackgroundColor;
                Console.BackgroundColor = value ? _color : ConsoleColor.Black;
                Console.WriteLine('O');
                Console.BackgroundColor = color;
            }
        }
    }
}