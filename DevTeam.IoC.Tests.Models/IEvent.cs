namespace DevTeam.IoC.Tests.Models
{
    internal interface IEvent<out T>
    {
        long Id { get; }

        T Data { get; }
    }
}