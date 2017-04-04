// ReSharper disable ArrangeTypeModifiers
namespace ConsoleApp
{
    using System;
    using DevTeam.IoC;
    using DevTeam.IoC.Contracts;

    public class Program
    {
        public static void Main()
        {
            using (var container = new Container()
                .Register().State<string>(0).Autowiring<IContact, Contact>().ToSelf())
            {
                var johnContact = container.Resolve().Instance<IContact>("John");
                Console.WriteLine(johnContact.Name);
            }
        }

        public Program(string text)
        {
            Console.WriteLine(text);
        }
    }

    interface IContact
    {
        string Name { get; }
    }

    class Contact : IContact
    {
        public Contact([State] string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
