namespace DevTeam.IoC.Contracts
{
    using System.Reflection;

    [PublicAPI]
    public interface IInstanceFactoryProvider
    {
        IInstanceFactory GetFactory(ConstructorInfo constructor);
    }
}