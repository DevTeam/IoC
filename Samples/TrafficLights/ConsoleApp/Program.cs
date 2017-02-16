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
                var container = new Container()
                    .Configure()
                        .DependsOn(Wellknown.Feature.Enumerables)
                    .ToSelf()
                    .Register()
                        .Contract<ILamp>().State<ConsoleColor>(0).Autowiring<LedLamp>()
                        .And().Contract<ITrafficLight>().Tag("pedestrian").Autowiring<PedestrianTrafficLight>()
                        .And().Contract<ITrafficLight>().Tag("standard").Autowiring<StandardTrafficLight>()
                        .And().Contract<Program>().Autowiring<Program>()
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