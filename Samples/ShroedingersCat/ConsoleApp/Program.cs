// ReSharper disable ArrangeTypeModifiers
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
            using (var container = new Container().Configure().DependsOn(new Glue()).ToSelf())
            {
                var box = container.Resolve().Instance<IBox<ICat>>();
            }
        }
    }

    class Glue: IConfiguration
    {
        public IEnumerable<IConfiguration> GetDependencies(IContainer container)
        {
            yield break;
        }

        public IEnumerable<IDisposable> Apply(IContainer container)
        {
            yield return container.Register()
                .Contract(typeof(IBox<>)).Autowiring(typeof(CardboardBox<>))
                .And().Contract<ICat>().Autowiring<ShroedingersCat>()
                .Apply();
        }
    }

    interface IBox<T> { T Content { get; } }

    interface ICat { }

    class CardboardBox<T> : IBox<T>
    {
        public CardboardBox(T content) { Content = content; }

        public T Content { get; }
    }

    class ShroedingersCat : ICat
    {
    }
}
