// ReSharper disable ArrangeTypeModifiers
// ReSharper disable ClassNeverInstantiated.Global
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
            using (var container = new Container().Configure().DependsOn<Glue>().ToSelf())
            {
                var box = container.Resolve().Instance<IBox<ICat>>();
                Console.WriteLine(box.Content.IsAlive);
            }
        }
    }

    interface IBox<out T> { T Content { get; } }

    interface ICat { bool IsAlive { get; } }

    class CardboardBox<T> : IBox<T>
    {
        public CardboardBox(T content) { Content = content; }

        public T Content { get; }
    }

    class ShroedingersCat : ICat
    {
        public bool IsAlive => true; // for humanistic reasons
    }

    class Glue : IConfiguration
    {
        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            yield return container.Register()
                .Autowiring(typeof(IBox<>), typeof(CardboardBox<>))
                .And().Autowiring<ICat, ShroedingersCat>();
        }
    }
}
