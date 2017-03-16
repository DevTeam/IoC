namespace DevTeam.IoC.Tests.Models
{
    internal interface IEvent<out T>
    {
        // ReSharper disable once UnusedMemberInSuper.Global
        long Id { get; }

        T Data { get; }
    }
}