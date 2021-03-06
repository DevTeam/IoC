﻿namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static void Main()
        {
            using (var container = new Container().Configure()
                    .DependsOn(Wellknown.Feature.Enumerables).ToSelf()
                    .Register()
                        .State<ConsoleColor>(0).Autowiring<ILamp, LedLamp>()
                        .And().Tag("pedestrian").Autowiring<ITrafficLight, PedestrianTrafficLight>()
                        .And().Tag("standard").Autowiring<ITrafficLight, StandardTrafficLight>()
                        .And().Autowiring<Program, Program>()
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