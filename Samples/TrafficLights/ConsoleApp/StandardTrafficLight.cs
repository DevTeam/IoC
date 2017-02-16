namespace ConsoleApp
{
    using System;
    using System.Threading;
    using DevTeam.IoC.Contracts;

    internal class StandardTrafficLight : ITrafficLight
    {
        private readonly ILamp _redLamp;
        private readonly ILamp _yellowLamp;
        private readonly ILamp _greenLamp;
        private bool _stop = true;

        public StandardTrafficLight(
            [State(0, typeof(ConsoleColor), Value = ConsoleColor.Red)] ILamp redLamp,
            [State(0, typeof(ConsoleColor), Value = ConsoleColor.Yellow)] ILamp yellowLamp,
            [State(0, typeof(ConsoleColor), Value = ConsoleColor.Green)] ILamp greenLamp)
        {
            _redLamp = redLamp;
            _yellowLamp = yellowLamp;
            _greenLamp = greenLamp;
        }

        public string Description => "Standard traffic light";

        public void ChangeState()
        {
            _redLamp.IsActive = false;
            _yellowLamp.IsActive = true;
            _greenLamp.IsActive = false;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine();

            _stop = !_stop;
            _redLamp.IsActive = _stop;
            _yellowLamp.IsActive = false;
            _greenLamp.IsActive = !_stop;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine();
        }
    }
}