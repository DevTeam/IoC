namespace ConsoleApp
{
    using System;
    using System.Threading;
    using DevTeam.IoC.Contracts;

    internal class PedestrianTrafficLight: ITrafficLight
    {
        private readonly ILamp _redLamp;
        private readonly ILamp _greenLamp;
        private bool _stop = true;

        public PedestrianTrafficLight(
            [State(0, typeof(ConsoleColor), Value = ConsoleColor.Red)] ILamp redLamp,
            [State(0, typeof(ConsoleColor), Value = ConsoleColor.Green)] ILamp greenLamp)
        {
            _redLamp = redLamp;
            _greenLamp = greenLamp;
        }

        public string Description => "Pedestrian traffic light";

        public void ChangeState()
        {
            _stop = !_stop;
            _redLamp.IsActive = _stop;
            _greenLamp.IsActive = !_stop;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Console.WriteLine();
        }
    }
}