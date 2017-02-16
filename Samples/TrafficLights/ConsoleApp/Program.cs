namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main()
        {
            using (
                var container = new Container().Configure()
                    .DependsOn(Wellknown.Feature.Enumerables)
                    .Register().Contract<ILamp>().State<ConsoleColor>(0).AsAutowiring<LedLamp>()
                    .Register().Contract<ITrafficLight>().Tag("pedestrian").AsAutowiring<PedestrianTrafficLight>()
                    .Register().Contract<ITrafficLight>().Tag("standard").AsAutowiring<StandardTrafficLight>()
                    .Register().Contract<Program>().AsAutowiring<Program>()
                    .ToSelf())
            {
                container.Resolve().Instance<Program>();
            }
        }

        internal Program(IEnumerable<ITrafficLight> trafficLights)
        {
            foreach (var trafficLight in trafficLights)
            {
                Console.WriteLine(trafficLight.Description);
                while (!Console.KeyAvailable)
                {
                    trafficLight.ChangeState();
                }

                Console.ReadLine();
            }
        }
    }
}