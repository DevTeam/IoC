// ReSharper disable ArrangeTypeModifiers
namespace ConsoleApp
{
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main()
        {
            using (var container = new Container()
                .Register().Contract(typeof(IBox<>)).Autowiring(typeof(CardboardBox<>))
                .And().Contract<ICat>().Autowiring<ShroedingersCat>().ToSelf())
            {
                var box = container.Resolve().Instance<IBox<ICat>>();
            }
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
